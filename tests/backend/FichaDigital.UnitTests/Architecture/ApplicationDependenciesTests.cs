using System.Reflection;
using System.Reflection.Emit;
using FichaDigital.Api.Modules.Fichas.Application;

namespace FichaDigital.UnitTests.Architecture;

public sealed class ApplicationDependenciesTests
{
    private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly Dictionary<short, OpCode> Opcodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode))
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opcode => opcode.Value);

    [Fact]
    public void Application_NaoDeveDependerDeEfInfraestruturaOuApi()
    {
        // Inclui os tipos gerados pelo compilador para métodos async e lambdas.
        var applicationTypes = typeof(RevisarFichaService).Assembly.GetTypes()
            .Where(type => type.Namespace?.Contains(".Modules.") == true &&
                (type.Namespace.EndsWith(".Application") || type.Namespace.Contains(".Application.")))
            .ToArray();
        Assert.NotEmpty(applicationTypes);

        var violations = new List<string>();
        foreach (var type in applicationTypes)
        {
            foreach (var dependency in Dependencies(type).SelectMany(Expand).Distinct())
            {
                var ns = dependency.Namespace ?? "";
                if (ns.StartsWith("Microsoft.EntityFrameworkCore") ||
                    ns.StartsWith("FichaDigital.Api.Infrastructure") ||
                    (ns.Contains(".Modules.") &&
                        (ns.Contains(".Infrastructure") || ns.EndsWith(".Api") || ns[(ns.IndexOf(".Modules.") + 9)..].Contains(".Api."))) ||
                    dependency == typeof(IQueryable) ||
                    (dependency.IsGenericType && dependency.GetGenericTypeDefinition() == typeof(IQueryable<>)))
                {
                    violations.Add($"{type.FullName} -> {dependency.FullName}");
                }
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<Type> Expand(Type type)
    {
        yield return type;
        if (type.HasElementType)
            foreach (var element in Expand(type.GetElementType()!)) yield return element;
        foreach (var argument in type.GetGenericArguments())
            foreach (var nested in Expand(argument)) yield return nested;
    }

    private static IEnumerable<Type> Dependencies(Type type)
    {
        if (type.BaseType is not null) yield return type.BaseType;
        foreach (var contract in type.GetInterfaces()) yield return contract;
        foreach (var field in type.GetFields(Members)) yield return field.FieldType;
        foreach (var method in type.GetMethods(Members).Cast<MethodBase>().Concat(type.GetConstructors(Members)))
        {
            if (method is MethodInfo info) yield return info.ReturnType;
            foreach (var parameter in method.GetParameters()) yield return parameter.ParameterType;
            var body = method.GetMethodBody();
            if (body is null) continue;
            foreach (var local in body.LocalVariables) yield return local.LocalType;
            var bytes = body.GetILAsByteArray()!;
            for (var offset = 0; offset < bytes.Length;)
            {
                var first = bytes[offset++];
                var opcode = Opcodes[first == 0xfe ? (short)(0xfe00 | bytes[offset++]) : first];
                if (opcode.OperandType is OperandType.InlineType or OperandType.InlineField or
                    OperandType.InlineMethod or OperandType.InlineTok)
                {
                    var member = method.Module.ResolveMember(BitConverter.ToInt32(bytes, offset),
                        type.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null);
                    if (member is Type referencedType) yield return referencedType;
                    else if (member?.DeclaringType is not null) yield return member.DeclaringType;
                    if (member is MethodInfo calledMethod)
                    {
                        yield return calledMethod.ReturnType;
                        foreach (var argument in calledMethod.GetGenericArguments()) yield return argument;
                    }
                }
                offset += opcode.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineSwitch => 4 + BitConverter.ToInt32(bytes, offset) * 4,
                    _ => 4
                };
            }
        }
    }
}

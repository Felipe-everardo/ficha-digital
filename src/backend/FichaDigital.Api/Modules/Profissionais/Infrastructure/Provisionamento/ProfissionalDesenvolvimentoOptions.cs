namespace FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;

public sealed class ProfissionalDesenvolvimentoOptions
{
    public const string Secao = "ProfissionalDesenvolvimento";

    public string? NomeCompleto { get; init; }

    public string? Email { get; init; }

    public string? Senha { get; init; }
}

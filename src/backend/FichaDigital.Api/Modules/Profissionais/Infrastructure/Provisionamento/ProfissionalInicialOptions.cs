namespace FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;

public sealed class ProfissionalInicialOptions
{
    public const string Secao = "ProfissionalInicial";

    public bool Habilitado { get; init; }

    public string? NomeCompleto { get; init; }

    public string? Email { get; init; }

    public string? Senha { get; init; }
}

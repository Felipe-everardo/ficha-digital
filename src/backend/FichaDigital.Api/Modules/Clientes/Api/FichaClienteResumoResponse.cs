namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed record FichaClienteResumoResponse(
    Guid Id,
    string Status,
    string TipoProcedimento,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    DateTimeOffset CriadaEmUtc);

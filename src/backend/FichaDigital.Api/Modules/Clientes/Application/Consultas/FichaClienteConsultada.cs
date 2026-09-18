namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed record FichaClienteConsultada(
    Guid Id,
    string Status,
    string TipoProcedimento,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    DateTimeOffset CriadaEmUtc);

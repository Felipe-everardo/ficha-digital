using FichaDigital.Api.Modules.Atendimentos.Api;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record FichaResumoResponse(
    Guid Id,
    Guid ClienteId,
    string ClienteNome,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    string TipoProcedimento,
    string Status,
    DateTimeOffset CriadaEmUtc,
    DateTimeOffset? ConcluidaEmUtc,
    AtendimentoResponse? Atendimento,
    DateTimeOffset? ConviteExpiraEmUtc,
    bool ConviteExpirado);

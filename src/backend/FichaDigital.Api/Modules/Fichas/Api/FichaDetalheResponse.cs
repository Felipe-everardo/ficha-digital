using FichaDigital.Api.Modules.Atendimentos.Api;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record FichaDetalheResponse(
    Guid Id,
    string Status,
    DateTimeOffset CriadaEmUtc,
    DateTimeOffset? ConviteExpiraEmUtc,
    bool ConviteExpirado,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    string TipoProcedimento,
    ClienteFichaDetalheResponse Cliente,
    QuestionarioSaudeDetalheResponse? QuestionarioSaude,
    AceiteTermoResumoResponse? AceiteTermo,
    AtendimentoResponse? Atendimento);

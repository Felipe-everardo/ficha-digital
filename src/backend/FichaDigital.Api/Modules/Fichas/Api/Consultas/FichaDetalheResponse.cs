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
    int? VersaoModelo,
    int? VersaoQuestionario,
    int? VersaoTermo,
    string? CnpjApresentado,
    ClienteFichaDetalheResponse Cliente,
    QuestionarioSaudeDetalheResponse? QuestionarioSaude,
    AceiteTermoResumoResponse? AceiteTermo,
    RevisaoProfissionalResponse? RevisaoProfissional,
    RegistroProcedimentoResponse? RegistroProcedimento);

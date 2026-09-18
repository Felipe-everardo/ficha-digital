namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record DetalheFichaConsultada(
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
    ClienteDaFichaConsultado Cliente,
    QuestionarioSaudeConsultado? QuestionarioSaude,
    AceiteTermoConsultado? AceiteTermo,
    RevisaoProfissionalConsultada? RevisaoProfissional,
    RegistroProcedimentoConsultado? RegistroProcedimento);

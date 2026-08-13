namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResultadoRespostaQuestionarioSaude(
    StatusRespostaQuestionarioSaude Resultado,
    Guid? QuestionarioId = null,
    Guid? FichaId = null,
    int? Versao = null,
    DateTimeOffset? RespondidoEmUtc = null);

using FichaDigital.Api.Modules.Fichas.Application;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Route("api/fichas/questionario-saude")]
[EnableRateLimiting(PoliticasRateLimitingFichas.ConvitesPublicos)]
public sealed class QuestionariosSaudeController(
    ResponderQuestionarioSaudeService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<QuestionarioSaudeRespondidoResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status410Gone)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<QuestionarioSaudeRespondidoResponse>> Responder(
        [FromBody] ResponderQuestionarioSaudeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResponderQuestionarioSaudeCommand(
            request.Token,
            request.TemDiabetes!.Value,
            request.TipoDiabetes,
            request.PossuiPressaoAlta!.Value,
            request.TemAlergia!.Value,
            request.DescricaoAlergia,
            request.PossuiCondicaoCardiaca!.Value,
            request.TemEpilepsia!.Value,
            request.TemHemofilia!.Value,
            request.UsaMarcaPasso!.Value,
            request.EstaGravidaOuAmamentando!.Value);

        var resultado = await service.ResponderAsync(
            command,
            cancellationToken);

        if (resultado.Resultado ==
            StatusRespostaQuestionarioSaude.Respondido)
        {
            var response = new QuestionarioSaudeRespondidoResponse(
                resultado.QuestionarioId!.Value,
                resultado.FichaId!.Value,
                resultado.Versao!.Value,
                resultado.RespondidoEmUtc!.Value);

            return Created(
                $"/api/fichas/{response.FichaId}/questionario-saude",
                response);
        }

        return resultado.Resultado switch
        {
            StatusRespostaQuestionarioSaude.ConviteExpirado => Problem(
                statusCode: StatusCodes.Status410Gone,
                title: "Convite expirado.",
                detail: "O prazo para responder esta ficha terminou."),

            StatusRespostaQuestionarioSaude.FichaIndisponivel => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ficha indisponível.",
                detail: "Esta ficha não está disponível para receber respostas."),

            StatusRespostaQuestionarioSaude.JaRespondido => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Questionário já respondido.",
                detail: "Esta ficha já possui um questionário de saúde."),

            _ => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Convite inválido.",
                detail: "Não foi encontrado um convite válido para o token informado.")
        };
    }
}

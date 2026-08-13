using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Route("api/fichas/termo-consentimento")]
[EnableRateLimiting(PoliticasRateLimitingFichas.ConvitesPublicos)]
public sealed class TermosConsentimentoController(
    AceitarTermoConsentimentoService service) : ControllerBase
{
    [HttpPost("aceitar")]
    [ProducesResponseType<TermoConsentimentoAceitoResponse>(
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
    public async Task<ActionResult<TermoConsentimentoAceitoResponse>> Aceitar(
        [FromBody] AceitarTermoConsentimentoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AceitarTermoConsentimentoCommand(
            request.Token,
            request.VersaoTermo!.Value,
            request.ConteudoHash,
            request.NomeAssinante);

        var resultado = await service.AceitarAsync(
            command,
            cancellationToken);

        if (resultado.Resultado == StatusAceiteTermoConsentimento.Aceito)
        {
            var response = new TermoConsentimentoAceitoResponse(
                resultado.AceiteId!.Value,
                resultado.FichaId!.Value,
                resultado.VersaoTermo!.Value,
                resultado.AceitoEmUtc!.Value,
                StatusFicha.Concluida.ToString());

            return Created(
                $"/api/fichas/{response.FichaId}/termo-consentimento",
                response);
        }

        return resultado.Resultado switch
        {
            StatusAceiteTermoConsentimento.ConviteExpirado => Problem(
                statusCode: StatusCodes.Status410Gone,
                title: "Convite expirado.",
                detail: "O prazo para concluir esta ficha terminou."),

            StatusAceiteTermoConsentimento.QuestionarioPendente => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Questionário pendente.",
                detail: "Responda o questionário antes de aceitar o termo."),

            StatusAceiteTermoConsentimento.TermoDesatualizado => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Termo atualizado.",
                detail: "O conteúdo do termo mudou. Recarregue a página e revise a versão atual."),

            StatusAceiteTermoConsentimento.JaAceito => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Termo já aceito.",
                detail: "Esta ficha já possui um aceite registrado."),

            StatusAceiteTermoConsentimento.FichaIndisponivel => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ficha indisponível.",
                detail: "Esta ficha não está disponível para conclusão."),

            _ => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Convite inválido.",
                detail: "Não foi encontrado um convite válido para o token informado.")
        };
    }
}

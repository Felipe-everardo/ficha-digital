using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Route("api/fichas/convites")]
[EnableRateLimiting(PoliticasRateLimitingFichas.ConvitesPublicos)]
public sealed class AberturaConvitesFichaController(
    AbrirConviteFichaService service,
    CalculadorHashConteudo calculadorHash) : ControllerBase
{
    [HttpPost("abrir")]
    [ProducesResponseType<ConviteFichaAbertoResponse>(
        StatusCodes.Status200OK)]
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
    public async Task<ActionResult<ConviteFichaAbertoResponse>> Abrir(
        [FromBody] AbrirConviteFichaRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await service.AbrirAsync(
            request.Token,
            cancellationToken);

        return resultado.Resultado switch
        {
            StatusAberturaConvite.Aberto => Ok(
                new ConviteFichaAbertoResponse(
                    resultado.FichaId!.Value,
                    resultado.StatusFicha!.Value.ToString(),
                    new TermoConsentimentoResponse(
                        TermoConsentimentoAtual.Versao,
                        TermoConsentimentoAtual.Conteudo,
                        calculadorHash.Calcular(
                            TermoConsentimentoAtual.Conteudo)))),

            StatusAberturaConvite.Expirado => Problem(
                statusCode: StatusCodes.Status410Gone,
                title: "Convite expirado.",
                detail: "O prazo para utilizar este convite terminou."),

            StatusAberturaConvite.Indisponivel => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ficha indisponível.",
                detail: "Esta ficha não está disponível para preenchimento."),

            _ => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Convite inválido.",
                detail: "Não foi encontrado um convite válido para o token informado.")
        };
    }
}

using FichaDigital.Api.Modules.Fichas.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Route("api/fichas/dados-pessoais")]
[EnableRateLimiting(PoliticasRateLimitingFichas.ConvitesPublicos)]
public sealed class DadosPessoaisController(
    PreencherDadosPessoaisService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<DadosPessoaisPreenchidosResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status410Gone)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DadosPessoaisPreenchidosResponse>> Preencher(
        [FromBody] PreencherDadosPessoaisRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PreencherDadosPessoaisCommand(
            request.Token,
            request.NomeCompleto,
            request.NomeSocial,
            request.Pronomes,
            request.DataNascimento!.Value,
            request.Celular,
            request.Email,
            request.Instagram,
            request.ContatoEmergenciaNome,
            request.ContatoEmergenciaCelular);
        var resultado = await service.PreencherAsync(
            command,
            cancellationToken);

        if (resultado.Resultado ==
            StatusPreenchimentoDadosPessoais.Preenchidos)
        {
            return Ok(new DadosPessoaisPreenchidosResponse(
                resultado.FichaId!.Value,
                resultado.ClienteId!.Value,
                resultado.NomeParaExibicao!,
                resultado.PreenchidosEmUtc!.Value));
        }

        return resultado.Resultado switch
        {
            StatusPreenchimentoDadosPessoais.ConviteExpirado => Problem(
                statusCode: StatusCodes.Status410Gone,
                title: "Convite expirado.",
                detail: "O prazo para preencher esta ficha terminou."),

            StatusPreenchimentoDadosPessoais.FichaIndisponivel => Problem(
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

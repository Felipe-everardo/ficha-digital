using FichaDigital.Api.Modules.Fichas.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/fichas")]
public sealed class FichasController(
    ListarFichasService listarFichasService,
    ObterFichaDetalheService obterFichaDetalheService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<FichasPaginadasResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FichasPaginadasResponse>> Listar(
        [FromQuery] ListarFichasRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await listarFichasService.ListarAsync(
            new FiltroConsultaFichas(
                request.Busca,
                request.TipoProcedimento,
                request.Status,
                request.CriadaDe,
                request.CriadaAte,
                request.ConcluidaDe,
                request.ConcluidaAte,
                request.Pagina,
                request.TamanhoPagina),
            cancellationToken);

        return Ok(resultado.ToResponse());
    }

    [HttpGet("{fichaId:guid}")]
    [ProducesResponseType<FichaDetalheResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FichaDetalheResponse>> ObterDetalhe(
        Guid fichaId,
        CancellationToken cancellationToken)
    {
        var ficha = await obterFichaDetalheService.ObterAsync(
            fichaId,
            cancellationToken);

        if (ficha is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Ficha não encontrada.",
                detail: "Não existe uma ficha com o identificador informado.");
        }

        return Ok(ficha.ToResponse());
    }
}

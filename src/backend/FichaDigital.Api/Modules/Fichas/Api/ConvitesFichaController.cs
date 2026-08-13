using FichaDigital.Api.Modules.Fichas.Application;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Route("api/clientes/{clienteId:guid}/fichas/convites")]
public sealed class ConvitesFichaController(
    EmitirConviteFichaService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ConviteFichaCriadoResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConviteFichaCriadoResponse>> Emitir(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        var conviteEmitido = await service.EmitirAsync(
            clienteId,
            cancellationToken);

        if (conviteEmitido is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Cliente não encontrado.",
                detail: "Não existe um cliente com o identificador informado.");
        }

        var response = new ConviteFichaCriadoResponse(
            conviteEmitido.FichaId,
            conviteEmitido.ConviteId,
            $"/fichas/preencher/{conviteEmitido.TokenOriginal}",
            conviteEmitido.ExpiraEmUtc);

        return Created(
            $"/api/fichas/{response.FichaId}/convites/{response.ConviteId}",
            response);
    }
}

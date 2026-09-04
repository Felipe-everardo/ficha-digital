using FichaDigital.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Features.Status;

[ApiController]
[Route("api/status")]
public sealed class StatusController(
    FichaDigitalDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<StatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<StatusResponse> Get()
    {
        var response = new StatusResponse(
            "Ficha Digital API",
            "Comunicação com o backend realizada com sucesso.",
            "0.1.0",
            DateTimeOffset.UtcNow);

        return Ok(response);
    }

    [HttpGet("database")]
    [ProducesResponseType<DatabaseStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<DatabaseStatusResponse>(
        StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<DatabaseStatusResponse>> GetDatabase(
        CancellationToken cancellationToken)
    {
        var conectado = await dbContext.Database.CanConnectAsync(
            cancellationToken);
        var response = new DatabaseStatusResponse(
            "Ficha Digital API",
            conectado ? "Connected" : "Unavailable",
            conectado
                ? "A conexão com o banco de dados está disponível."
                : "Não foi possível conectar ao banco de dados.",
            DateTimeOffset.UtcNow);

        return conectado
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}

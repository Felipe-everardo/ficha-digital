using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Features.Status;

[ApiController]
[Route("api/status")]
public sealed class StatusController : ControllerBase
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
}

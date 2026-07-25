using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Clientes.Api;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(
    FichaDigitalDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ClienteCriadoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteCriadoResponse>> Criar(
        [FromBody] CriarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var dataNascimento = request.DataNascimento!.Value;

        if (dataNascimento > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            ModelState.AddModelError(
                nameof(request.DataNascimento),
                "A data de nascimento não pode estar no futuro.");

            return ValidationProblem(ModelState);
        }

        var cliente = new Cliente(
            request.NomeCompleto,
            request.NomeSocial,
            request.Pronomes,
            dataNascimento,
            request.Celular,
            request.Email);

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new ClienteCriadoResponse(
            cliente.Id,
            cliente.NomeParaExibicao,
            cliente.Pronomes,
            cliente.CriadoEmUtc);

        return Created($"/api/clientes/{cliente.Id}", response);
    }
}

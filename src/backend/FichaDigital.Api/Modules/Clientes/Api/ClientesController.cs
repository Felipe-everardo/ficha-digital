using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Clientes.Api;

[ApiController]
[Authorize]
[Route("api/clientes")]
public sealed class ClientesController(
    FichaDigitalDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ClientesPaginadosResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientesPaginadosResponse>> Listar(
        [FromQuery] ListarClientesRequest request,
        CancellationToken cancellationToken)
    {
        var consulta = dbContext.Clientes.AsNoTracking();
        var totalItens = await consulta.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(
            totalItens / (double)request.TamanhoPagina);
        var itens = await consulta
            .OrderBy(cliente =>
                cliente.NomeSocial ??
                cliente.NomeCompleto ??
                cliente.NomeReferencia)
            .ThenBy(cliente => cliente.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(cliente => new ClienteResumoResponse(
                cliente.Id,
                cliente.NomeReferencia,
                cliente.NomeCompleto,
                cliente.NomeSocial ??
                    cliente.NomeCompleto ??
                    cliente.NomeReferencia,
                cliente.Pronomes,
                cliente.Celular,
                cliente.Email,
                cliente.Instagram,
                cliente.DadosPessoaisPreenchidosEmUtc != null,
                cliente.CriadoEmUtc))
            .ToListAsync(cancellationToken);

        return Ok(new ClientesPaginadosResponse(
            itens,
            request.Pagina,
            request.TamanhoPagina,
            totalItens,
            totalPaginas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<ClienteCriadoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteCriadoResponse>> Criar(
        [FromBody] CriarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = new Cliente(request.NomeReferencia);

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new ClienteCriadoResponse(
            cliente.Id,
            cliente.NomeParaExibicao,
            cliente.CriadoEmUtc);

        return Created($"/api/clientes/{cliente.Id}", response);
    }
}

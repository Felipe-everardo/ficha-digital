using FichaDigital.Api.Modules.Clientes.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Clientes.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/clientes")]
public sealed class ClientesController(
    CriarClienteService criarClienteService,
    ConsultaClientes consultaClientes) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ClientesPaginadosResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientesPaginadosResponse>> Listar(
        [FromQuery] ListarClientesRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await consultaClientes.ListarAsync(
            new FiltroConsultaClientes(
                request.Busca,
                request.Telefone,
                request.Instagram,
                request.TipoProcedimento,
                request.UltimaFichaDe,
                request.UltimaFichaAte,
                request.Pagina,
                request.TamanhoPagina),
            cancellationToken);

        return Ok(resultado.ToResponse());
    }

    [HttpGet("{clienteId:guid}")]
    [ProducesResponseType<ClienteDetalheResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteDetalheResponse>> ObterDetalhe(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        var cliente = await consultaClientes.ObterDetalheAsync(
            clienteId,
            cancellationToken);

        if (cliente is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Cliente não encontrado.",
                detail: "Não existe um cliente com o identificador informado.");
        }

        return Ok(cliente.ToResponse());
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
        var cliente = await criarClienteService.CriarAsync(
            new CriarClienteCommand(request.NomeReferencia),
            cancellationToken);
        var response = cliente.ToResponse();

        return Created($"/api/clientes/{response.Id}", response);
    }
}

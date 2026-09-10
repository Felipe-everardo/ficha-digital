using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Application;
using FichaDigital.Api.Modules.Clientes.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Clientes.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/clientes")]
public sealed class ClientesController(
    FichaDigitalDbContext dbContext,
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
                request.ProfissionalId,
                request.TipoProcedimento,
                request.UltimaFichaDe,
                request.UltimaFichaAte,
                request.Pagina,
                request.TamanhoPagina),
            cancellationToken);

        return Ok(new ClientesPaginadosResponse(
            resultado.Itens.Select(CriarResumoCliente).ToList(),
            resultado.Pagina,
            resultado.TamanhoPagina,
            resultado.TotalItens,
            resultado.TotalPaginas));
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

        return Ok(new ClienteDetalheResponse(
            cliente.Id,
            cliente.NomeReferencia,
            cliente.NomeCompleto,
            cliente.NomeSocial,
            cliente.NomeParaExibicao,
            cliente.Pronomes,
            cliente.DataNascimento,
            cliente.Celular,
            cliente.Email,
            cliente.Instagram,
            cliente.ContatoEmergenciaNome,
            cliente.ContatoEmergenciaCelular,
            cliente.DadosPessoaisPreenchidosEmUtc,
            cliente.CriadoEmUtc,
            cliente.Fichas.Select(CriarResumoFicha).ToList()));
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

    private static ClienteResumoResponse CriarResumoCliente(
        ClienteConsultado cliente)
    {
        return new ClienteResumoResponse(
            cliente.Id,
            cliente.NomeReferencia,
            cliente.NomeCompleto,
            cliente.NomeParaExibicao,
            cliente.Pronomes,
            cliente.Celular,
            cliente.Email,
            cliente.Instagram,
            cliente.CriadoEmUtc,
            cliente.UltimaFicha is null
                ? null
                : CriarResumoFicha(cliente.UltimaFicha));
    }

    private static FichaClienteResumoResponse CriarResumoFicha(
        FichaClienteConsultada ficha)
    {
        return new FichaClienteResumoResponse(
            ficha.Id,
            ficha.Status,
            ficha.TipoProcedimento,
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.CriadaEmUtc);
    }
}

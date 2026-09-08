using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Clientes.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.Trim();
            consulta = consulta.Where(cliente =>
                cliente.NomeReferencia.StartsWith(busca) ||
                (cliente.NomeCompleto != null &&
                 cliente.NomeCompleto.StartsWith(busca)) ||
                (cliente.NomeSocial != null &&
                 cliente.NomeSocial.StartsWith(busca)));
        }

        var inicioAtendimento = request.AtendimentoDe is null
            ? (DateTimeOffset?)null
            : new DateTimeOffset(
                request.AtendimentoDe.Value.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);
        var fimAtendimentoExclusivo = request.AtendimentoAte is null ||
            request.AtendimentoAte.Value == DateOnly.MaxValue
                ? (DateTimeOffset?)null
                : new DateTimeOffset(
                    request.AtendimentoAte.Value
                        .AddDays(1)
                        .ToDateTime(TimeOnly.MinValue),
                    TimeSpan.Zero);
        var possuiFiltrosAtendimento =
            request.ProfissionalId is not null ||
            request.TipoProcedimento is not null ||
            inicioAtendimento is not null ||
            fimAtendimentoExclusivo is not null;

        Dictionary<Guid, Ficha> ultimasFichas;
        List<Cliente> clientes;
        int totalItens;

        if (possuiFiltrosAtendimento && string.Equals(
                dbContext.Database.ProviderName,
                "Microsoft.EntityFrameworkCore.Sqlite",
                StringComparison.Ordinal))
        {
            // SQLite é utilizado somente nos testes e não ordena DateTimeOffset.
            var candidatos = await consulta
                .OrderBy(cliente =>
                    cliente.NomeSocial ??
                    cliente.NomeCompleto ??
                    cliente.NomeReferencia)
                .ThenBy(cliente => cliente.Id)
                .ToListAsync(cancellationToken);
            ultimasFichas = await CarregarUltimasFichasAsync(
                candidatos.Select(cliente => cliente.Id),
                cancellationToken);
            var clientesFiltrados = candidatos
                .Where(cliente =>
                    ultimasFichas.TryGetValue(cliente.Id, out var ficha) &&
                    CorrespondeAoAtendimento(
                        ficha,
                        request,
                        inicioAtendimento,
                        fimAtendimentoExclusivo))
                .ToList();

            totalItens = clientesFiltrados.Count;
            clientes = clientesFiltrados
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToList();
        }
        else
        {
            if (possuiFiltrosAtendimento)
            {
                consulta = consulta.Where(cliente =>
                    dbContext.Fichas
                        .Where(ficha => ficha.ClienteId == cliente.Id)
                        .OrderByDescending(ficha => ficha.CriadaEmUtc)
                        .ThenByDescending(ficha => ficha.Id)
                        .Take(1)
                        .Any(ficha =>
                            (request.ProfissionalId == null ||
                             ficha.ProfissionalResponsavelId ==
                             request.ProfissionalId) &&
                            (request.TipoProcedimento == null ||
                             ficha.TipoProcedimento ==
                             request.TipoProcedimento) &&
                            (inicioAtendimento == null ||
                             ficha.CriadaEmUtc >= inicioAtendimento.Value) &&
                            (fimAtendimentoExclusivo == null ||
                             ficha.CriadaEmUtc <
                             fimAtendimentoExclusivo.Value)));
            }

            totalItens = await consulta.CountAsync(cancellationToken);
            clientes = await consulta
                .OrderBy(cliente =>
                    cliente.NomeSocial ??
                    cliente.NomeCompleto ??
                    cliente.NomeReferencia)
                .ThenBy(cliente => cliente.Id)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            ultimasFichas = await CarregarUltimasFichasAsync(
                clientes.Select(cliente => cliente.Id),
                cancellationToken);
        }

        var totalPaginas = (int)Math.Ceiling(
            totalItens / (double)request.TamanhoPagina);
        var itens = clientes
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
                cliente.CriadoEmUtc,
                ultimasFichas.TryGetValue(cliente.Id, out var ultimaFicha)
                    ? CriarResumoFicha(ultimaFicha)
                    : null))
            .ToList();

        return Ok(new ClientesPaginadosResponse(
            itens,
            request.Pagina,
            request.TamanhoPagina,
            totalItens,
            totalPaginas));
    }

    private async Task<Dictionary<Guid, Ficha>> CarregarUltimasFichasAsync(
        IEnumerable<Guid> clienteIds,
        CancellationToken cancellationToken)
    {
        var ids = clienteIds.ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        var fichas = await dbContext.Fichas
            .AsNoTracking()
            .Where(ficha => ids.Contains(ficha.ClienteId))
            .ToListAsync(cancellationToken);

        return fichas
            .GroupBy(ficha => ficha.ClienteId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo
                    .OrderByDescending(ficha => ficha.CriadaEmUtc)
                    .ThenByDescending(ficha => ficha.Id)
                    .First());
    }

    private static bool CorrespondeAoAtendimento(
        Ficha ficha,
        ListarClientesRequest request,
        DateTimeOffset? inicio,
        DateTimeOffset? fimExclusivo)
    {
        return (request.ProfissionalId is null ||
                ficha.ProfissionalResponsavelId == request.ProfissionalId) &&
            (request.TipoProcedimento is null ||
             ficha.TipoProcedimento == request.TipoProcedimento) &&
            (inicio is null || ficha.CriadaEmUtc >= inicio) &&
            (fimExclusivo is null || ficha.CriadaEmUtc < fimExclusivo);
    }

    private static FichaClienteResumoResponse CriarResumoFicha(Ficha ficha)
    {
        return new FichaClienteResumoResponse(
            ficha.Id,
            ficha.Status.ToString(),
            ficha.TipoProcedimento.ToString(),
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.CriadaEmUtc);
    }

    [HttpGet("{clienteId:guid}")]
    [ProducesResponseType<ClienteDetalheResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteDetalheResponse>> ObterDetalhe(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == clienteId,
                cancellationToken);

        if (cliente is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Cliente não encontrado.",
                detail: "Não existe um cliente com o identificador informado.");
        }

        var fichasEncontradas = await dbContext.Fichas
            .AsNoTracking()
            .Where(ficha => ficha.ClienteId == clienteId)
            .ToListAsync(cancellationToken);
        var fichas = fichasEncontradas
            .Select(ficha => new FichaClienteResumoResponse(
                ficha.Id,
                ficha.Status.ToString(),
                ficha.TipoProcedimento.ToString(),
                ficha.ProfissionalResponsavelId,
                ficha.ProfissionalResponsavelNome,
                ficha.CriadaEmUtc))
            .OrderByDescending(ficha => ficha.CriadaEmUtc)
            .ThenBy(ficha => ficha.Id)
            .ToList();

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
            fichas));
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

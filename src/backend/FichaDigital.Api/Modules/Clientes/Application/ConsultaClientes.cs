using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Clientes.Application;

public sealed record FiltroConsultaClientes(
    string? Busca,
    Guid? ProfissionalId,
    TipoProcedimento? TipoProcedimento,
    DateOnly? UltimaFichaDe,
    DateOnly? UltimaFichaAte,
    int Pagina,
    int TamanhoPagina);

public sealed record FichaClienteConsultada(
    Guid Id,
    string Status,
    string TipoProcedimento,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    DateTimeOffset CriadaEmUtc);

public sealed record ClienteConsultado(
    Guid Id,
    string NomeReferencia,
    string? NomeCompleto,
    string NomeParaExibicao,
    string? Pronomes,
    string? Celular,
    string? Email,
    string? Instagram,
    DateTimeOffset CriadoEmUtc,
    FichaClienteConsultada? UltimaFicha);

public sealed record PaginaClientesConsultada(
    IReadOnlyList<ClienteConsultado> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);

public sealed record DetalheClienteConsultado(
    Guid Id,
    string NomeReferencia,
    string? NomeCompleto,
    string? NomeSocial,
    string NomeParaExibicao,
    string? Pronomes,
    DateOnly? DataNascimento,
    string? Celular,
    string? Email,
    string? Instagram,
    string? ContatoEmergenciaNome,
    string? ContatoEmergenciaCelular,
    DateTimeOffset? DadosPessoaisPreenchidosEmUtc,
    DateTimeOffset CriadoEmUtc,
    IReadOnlyList<FichaClienteConsultada> Fichas);

public sealed class ConsultaClientes(FichaDigitalDbContext dbContext)
{
    public async Task<PaginaClientesConsultada> ListarAsync(
        FiltroConsultaClientes filtro,
        CancellationToken cancellationToken)
    {
        var consulta = dbContext.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            consulta = consulta.Where(cliente =>
                cliente.NomeReferencia.StartsWith(busca) ||
                (cliente.NomeCompleto != null &&
                 cliente.NomeCompleto.StartsWith(busca)) ||
                (cliente.NomeSocial != null &&
                 cliente.NomeSocial.StartsWith(busca)));
        }

        var inicioUltimaFicha = filtro.UltimaFichaDe is null
            ? (DateTimeOffset?)null
            : new DateTimeOffset(
                filtro.UltimaFichaDe.Value.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);
        var fimUltimaFichaExclusivo = filtro.UltimaFichaAte is null ||
            filtro.UltimaFichaAte.Value == DateOnly.MaxValue
                ? (DateTimeOffset?)null
                : new DateTimeOffset(
                    filtro.UltimaFichaAte.Value
                        .AddDays(1)
                        .ToDateTime(TimeOnly.MinValue),
                    TimeSpan.Zero);
        var possuiFiltrosUltimaFicha =
            filtro.ProfissionalId is not null ||
            filtro.TipoProcedimento is not null ||
            inicioUltimaFicha is not null ||
            fimUltimaFichaExclusivo is not null;

        Dictionary<Guid, Ficha> ultimasFichas;
        List<Cliente> clientes;
        int totalItens;

        if (possuiFiltrosUltimaFicha && string.Equals(
                dbContext.Database.ProviderName,
                "Microsoft.EntityFrameworkCore.Sqlite",
                StringComparison.Ordinal))
        {
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
                    CorrespondeAUltimaFicha(
                        ficha,
                        filtro,
                        inicioUltimaFicha,
                        fimUltimaFichaExclusivo))
                .ToList();

            totalItens = clientesFiltrados.Count;
            clientes = clientesFiltrados
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .ToList();
        }
        else
        {
            if (possuiFiltrosUltimaFicha)
            {
                consulta = consulta.Where(cliente =>
                    dbContext.Fichas
                        .Where(ficha => ficha.ClienteId == cliente.Id)
                        .OrderByDescending(ficha => ficha.CriadaEmUtc)
                        .ThenByDescending(ficha => ficha.Id)
                        .Take(1)
                        .Any(ficha =>
                            (filtro.ProfissionalId == null ||
                             ficha.ProfissionalResponsavelId ==
                             filtro.ProfissionalId) &&
                            (filtro.TipoProcedimento == null ||
                             ficha.TipoProcedimento ==
                             filtro.TipoProcedimento) &&
                            (inicioUltimaFicha == null ||
                             ficha.CriadaEmUtc >= inicioUltimaFicha.Value) &&
                            (fimUltimaFichaExclusivo == null ||
                             ficha.CriadaEmUtc <
                             fimUltimaFichaExclusivo.Value)));
            }

            totalItens = await consulta.CountAsync(cancellationToken);
            clientes = await consulta
                .OrderBy(cliente =>
                    cliente.NomeSocial ??
                    cliente.NomeCompleto ??
                    cliente.NomeReferencia)
                .ThenBy(cliente => cliente.Id)
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .ToListAsync(cancellationToken);
            ultimasFichas = await CarregarUltimasFichasAsync(
                clientes.Select(cliente => cliente.Id),
                cancellationToken);
        }

        var totalPaginas = (int)Math.Ceiling(
            totalItens / (double)filtro.TamanhoPagina);
        var itens = clientes
            .Select(cliente => new ClienteConsultado(
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

        return new PaginaClientesConsultada(
            itens,
            filtro.Pagina,
            filtro.TamanhoPagina,
            totalItens,
            totalPaginas);
    }

    public async Task<DetalheClienteConsultado?> ObterDetalheAsync(
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
            return null;
        }

        var fichasEncontradas = await dbContext.Fichas
            .AsNoTracking()
            .Where(ficha => ficha.ClienteId == clienteId)
            .ToListAsync(cancellationToken);
        var fichas = fichasEncontradas
            .Select(CriarResumoFicha)
            .OrderByDescending(ficha => ficha.CriadaEmUtc)
            .ThenBy(ficha => ficha.Id)
            .ToList();

        return new DetalheClienteConsultado(
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
            fichas);
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

    private static bool CorrespondeAUltimaFicha(
        Ficha ficha,
        FiltroConsultaClientes filtro,
        DateTimeOffset? inicio,
        DateTimeOffset? fimExclusivo)
    {
        return (filtro.ProfissionalId is null ||
                ficha.ProfissionalResponsavelId == filtro.ProfissionalId) &&
            (filtro.TipoProcedimento is null ||
             ficha.TipoProcedimento == filtro.TipoProcedimento) &&
            (inicio is null || ficha.CriadaEmUtc >= inicio) &&
            (fimExclusivo is null || ficha.CriadaEmUtc < fimExclusivo);
    }

    private static FichaClienteConsultada CriarResumoFicha(Ficha ficha)
    {
        return new FichaClienteConsultada(
            ficha.Id,
            ficha.Status.ToString(),
            ficha.TipoProcedimento.ToString(),
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.CriadaEmUtc);
    }
}

using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class ListarFichasService(
    FichaDigitalDbContext dbContext,
    TimeProvider timeProvider)
{
    public async Task<PaginaFichasConsultada> ListarAsync(
        FiltroConsultaFichas filtro,
        CancellationToken cancellationToken)
    {
        var consulta =
            from ficha in dbContext.Fichas.AsNoTracking()
            join cliente in dbContext.Clientes.AsNoTracking()
                on ficha.ClienteId equals cliente.Id
            join dados in dbContext.DadosPessoaisFichas.AsNoTracking()
                on ficha.Id equals dados.FichaId into dadosPessoais
            from dados in dadosPessoais.DefaultIfEmpty()
            select new
            {
                ficha.Id,
                ficha.ClienteId,
                ficha.ProfissionalResponsavelId,
                ficha.ProfissionalResponsavelNome,
                ficha.TipoProcedimento,
                ClienteNome = dados == null
                    ? cliente.NomeReferencia
                    : dados.NomeSocial ?? dados.NomeCompleto,
                DadosNomeCompleto = dados == null ? null : dados.NomeCompleto,
                DadosNomeSocial = dados == null ? null : dados.NomeSocial,
                ClienteNomeAtual = cliente.NomeSocial ??
                    cliente.NomeCompleto ??
                    cliente.NomeReferencia,
                cliente.NomeReferencia,
                cliente.NomeCompleto,
                cliente.NomeSocial,
                cliente.Email,
                cliente.Celular,
                ficha.Status,
                ficha.CriadaEmUtc,
                ConcluidaEmUtc = dbContext.RegistrosTatuagem
                    .Where(registro => registro.FichaId == ficha.Id)
                    .Select(registro =>
                        (DateTimeOffset?)registro.RegistradoEmUtc)
                    .SingleOrDefault() ??
                    dbContext.RegistrosPiercing
                        .Where(registro => registro.FichaId == ficha.Id)
                        .Select(registro =>
                            (DateTimeOffset?)registro.RegistradoEmUtc)
                        .SingleOrDefault() ??
                    (ficha.VersaoModelo == null &&
                     ficha.Status == StatusFicha.Concluida
                        ? dbContext.AceitesTermoConsentimento
                            .Where(aceite => aceite.FichaId == ficha.Id)
                            .Select(aceite =>
                                (DateTimeOffset?)aceite.AceitoEmUtc)
                            .SingleOrDefault()
                        : null),
                ConviteExpiraEmUtc = dbContext.ConvitesFicha
                    .Where(convite => convite.FichaId == ficha.Id)
                    .Select(convite => (DateTimeOffset?)convite.ExpiraEmUtc)
                    .SingleOrDefault()
            };

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            consulta = consulta.Where(ficha =>
                ficha.ClienteNome.Contains(busca) ||
                (ficha.DadosNomeCompleto != null &&
                    ficha.DadosNomeCompleto.Contains(busca)) ||
                (ficha.DadosNomeSocial != null &&
                    ficha.DadosNomeSocial.Contains(busca)) ||
                ficha.ClienteNomeAtual.Contains(busca) ||
                ficha.NomeReferencia.Contains(busca) ||
                (ficha.NomeCompleto != null &&
                    ficha.NomeCompleto.Contains(busca)) ||
                (ficha.NomeSocial != null &&
                    ficha.NomeSocial.Contains(busca)) ||
                (ficha.Email != null && ficha.Email.Contains(busca)) ||
                (ficha.Celular != null && ficha.Celular.Contains(busca)) ||
                ficha.ProfissionalResponsavelNome.Contains(busca));
        }

        if (filtro.TipoProcedimento is not null)
        {
            consulta = consulta.Where(ficha =>
                ficha.TipoProcedimento == filtro.TipoProcedimento);
        }

        if (filtro.Status is not null)
        {
            consulta = consulta.Where(ficha => ficha.Status == filtro.Status);
        }

        var providerIsSqlite = dbContext.Database.ProviderName?.Contains(
            "Sqlite",
            StringComparison.OrdinalIgnoreCase) == true;

        if (providerIsSqlite &&
            (filtro.CriadaDe is not null || filtro.CriadaAte is not null))
        {
            var fichasPorData = await dbContext.Fichas
                .AsNoTracking()
                .Select(ficha => new { ficha.Id, ficha.CriadaEmUtc })
                .ToListAsync(cancellationToken);
            var ids = fichasPorData
                .Where(ficha =>
                    (filtro.CriadaDe is null ||
                        ficha.CriadaEmUtc >= InicioUtc(filtro.CriadaDe.Value)) &&
                    (filtro.CriadaAte is null ||
                        filtro.CriadaAte == DateOnly.MaxValue ||
                        ficha.CriadaEmUtc < InicioUtc(
                            filtro.CriadaAte.Value.AddDays(1))))
                .Select(ficha => ficha.Id)
                .ToList();
            consulta = consulta.Where(ficha => ids.Contains(ficha.Id));
        }
        else if (filtro.CriadaDe is not null)
        {
            var inicio = InicioUtc(filtro.CriadaDe.Value);
            consulta = consulta.Where(ficha => ficha.CriadaEmUtc >= inicio);
        }

        if (!providerIsSqlite &&
            filtro.CriadaAte is not null &&
            filtro.CriadaAte != DateOnly.MaxValue)
        {
            var fimExclusivo = InicioUtc(filtro.CriadaAte.Value.AddDays(1));
            consulta = consulta.Where(ficha =>
                ficha.CriadaEmUtc < fimExclusivo);
        }

        if (providerIsSqlite &&
            (filtro.ConcluidaDe is not null || filtro.ConcluidaAte is not null))
        {
            var tatuagensPorData = await dbContext.RegistrosTatuagem
                .AsNoTracking()
                .Select(registro => new
                {
                    registro.FichaId,
                    ConcluidaEmUtc = registro.RegistradoEmUtc
                })
                .ToListAsync(cancellationToken);
            var piercingsPorData = await dbContext.RegistrosPiercing
                .AsNoTracking()
                .Select(registro => new
                {
                    registro.FichaId,
                    ConcluidaEmUtc = registro.RegistradoEmUtc
                })
                .ToListAsync(cancellationToken);
            var legadosPorData = await (
                from aceite in dbContext.AceitesTermoConsentimento.AsNoTracking()
                join ficha in dbContext.Fichas.AsNoTracking()
                    on aceite.FichaId equals ficha.Id
                where ficha.VersaoModelo == null &&
                    ficha.Status == StatusFicha.Concluida
                select new
                {
                    aceite.FichaId,
                    ConcluidaEmUtc = aceite.AceitoEmUtc
                }).ToListAsync(cancellationToken);
            var ids = tatuagensPorData
                .Concat(piercingsPorData)
                .Concat(legadosPorData)
                .Where(registro =>
                    (filtro.ConcluidaDe is null ||
                        registro.ConcluidaEmUtc >= InicioUtc(
                            filtro.ConcluidaDe.Value)) &&
                    (filtro.ConcluidaAte is null ||
                        filtro.ConcluidaAte == DateOnly.MaxValue ||
                        registro.ConcluidaEmUtc < InicioUtc(
                            filtro.ConcluidaAte.Value.AddDays(1))))
                .Select(registro => registro.FichaId)
                .ToList();
            consulta = consulta.Where(ficha => ids.Contains(ficha.Id));
        }
        else if (filtro.ConcluidaDe is not null)
        {
            var inicio = InicioUtc(filtro.ConcluidaDe.Value);
            consulta = consulta.Where(ficha => ficha.ConcluidaEmUtc >= inicio);
        }

        if (!providerIsSqlite &&
            filtro.ConcluidaAte is not null &&
            filtro.ConcluidaAte != DateOnly.MaxValue)
        {
            var fimExclusivo = InicioUtc(filtro.ConcluidaAte.Value.AddDays(1));
            consulta = consulta.Where(ficha =>
                ficha.ConcluidaEmUtc < fimExclusivo);
        }

        var totalItens = await consulta.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(
            totalItens / (double)filtro.TamanhoPagina);
        var ordenarPorConclusao =
            filtro.ConcluidaDe is not null || filtro.ConcluidaAte is not null;
        var deslocamento = (filtro.Pagina - 1) * filtro.TamanhoPagina;
        var registros = providerIsSqlite
            ? (ordenarPorConclusao
                ? (await consulta.ToListAsync(cancellationToken))
                    .OrderByDescending(ficha => ficha.ConcluidaEmUtc)
                    .ThenByDescending(ficha => ficha.CriadaEmUtc)
                    .ThenBy(ficha => ficha.Id)
                : (await consulta.ToListAsync(cancellationToken))
                    .OrderByDescending(ficha => ficha.CriadaEmUtc)
                    .ThenBy(ficha => ficha.Id))
                .Skip(deslocamento)
                .Take(filtro.TamanhoPagina)
                .ToList()
            : await (ordenarPorConclusao
                    ? consulta
                        .OrderByDescending(ficha => ficha.ConcluidaEmUtc)
                        .ThenByDescending(ficha => ficha.CriadaEmUtc)
                        .ThenBy(ficha => ficha.Id)
                    : consulta
                        .OrderByDescending(ficha => ficha.CriadaEmUtc)
                        .ThenBy(ficha => ficha.Id))
                .Skip(deslocamento)
                .Take(filtro.TamanhoPagina)
                .ToListAsync(cancellationToken);
        var instanteAtual = timeProvider.GetUtcNow();
        var itens = registros
            .Select(ficha => new FichaConsultada(
                ficha.Id,
                ficha.ClienteId,
                ficha.ClienteNome,
                ficha.ProfissionalResponsavelId,
                ficha.ProfissionalResponsavelNome,
                ficha.TipoProcedimento.ToString(),
                ficha.Status.ToString(),
                ficha.CriadaEmUtc,
                ficha.ConcluidaEmUtc,
                ficha.ConviteExpiraEmUtc,
                ficha.ConviteExpiraEmUtc is not null &&
                    ficha.ConviteExpiraEmUtc <= instanteAtual))
            .ToList();

        return new PaginaFichasConsultada(
            itens,
            filtro.Pagina,
            filtro.TamanhoPagina,
            totalItens,
            totalPaginas);
    }

    private static DateTimeOffset InicioUtc(DateOnly data)
    {
        return HorarioEstudio.ObterInicioUtc(data);
    }
}

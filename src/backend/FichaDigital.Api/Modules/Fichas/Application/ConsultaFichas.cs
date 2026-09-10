using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record FiltroConsultaFichas(
    string? Busca,
    Guid? ProfissionalId,
    TipoProcedimento? TipoProcedimento,
    StatusFicha? Status,
    DateOnly? CriadaDe,
    DateOnly? CriadaAte,
    DateOnly? ConcluidaDe,
    DateOnly? ConcluidaAte,
    int Pagina,
    int TamanhoPagina);

public sealed record FichaConsultada(
    Guid Id,
    Guid ClienteId,
    string ClienteNome,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    string TipoProcedimento,
    string Status,
    DateTimeOffset CriadaEmUtc,
    DateTimeOffset? ConcluidaEmUtc,
    DateTimeOffset? ConviteExpiraEmUtc,
    bool ConviteExpirado);

public sealed record PaginaFichasConsultada(
    IReadOnlyList<FichaConsultada> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);

public sealed record ClienteDaFichaConsultado(
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
    DateTimeOffset? DadosPessoaisPreenchidosEmUtc);

public sealed record QuestionarioSaudeConsultado(
    int Versao,
    bool TemDiabetes,
    string? TipoDiabetes,
    bool PossuiPressaoAlta,
    bool TemAlergia,
    string? DescricaoAlergia,
    bool PossuiCondicaoCardiaca,
    bool TemEpilepsia,
    bool TemHemofilia,
    bool UsaMarcaPasso,
    bool EstaGravidaOuAmamentando,
    DateTimeOffset RespondidoEmUtc);

public sealed record AceiteTermoConsultado(
    int VersaoTermo,
    string NomeAssinante,
    DateTimeOffset AceitoEmUtc,
    bool ConfirmouMaioridade,
    bool ConfirmouDadosPessoais,
    bool ConfirmouQuestionarioSaude,
    string EvidenciaHash,
    bool EvidenciaIntegra);

public sealed record DetalheFichaConsultada(
    Guid Id,
    string Status,
    DateTimeOffset CriadaEmUtc,
    DateTimeOffset? ConviteExpiraEmUtc,
    bool ConviteExpirado,
    Guid? ProfissionalResponsavelId,
    string ProfissionalResponsavelNome,
    string TipoProcedimento,
    ClienteDaFichaConsultado Cliente,
    QuestionarioSaudeConsultado? QuestionarioSaude,
    AceiteTermoConsultado? AceiteTermo);

public sealed class ConsultaFichas(
    FichaDigitalDbContext dbContext,
    TimeProvider timeProvider,
    CalculadorHashConteudo calculadorHash)
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
            join aceite in dbContext.AceitesTermoConsentimento.AsNoTracking()
                on ficha.Id equals aceite.FichaId into aceites
            from aceite in aceites.DefaultIfEmpty()
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
                ConcluidaEmUtc = aceite == null
                    ? null
                    : (DateTimeOffset?)aceite.AceitoEmUtc,
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

        if (filtro.ProfissionalId is not null)
        {
            consulta = consulta.Where(ficha =>
                ficha.ProfissionalResponsavelId == filtro.ProfissionalId);
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
            var aceitesPorData = await dbContext.AceitesTermoConsentimento
                .AsNoTracking()
                .Select(aceite => new { aceite.FichaId, aceite.AceitoEmUtc })
                .ToListAsync(cancellationToken);
            var ids = aceitesPorData
                .Where(aceite =>
                    (filtro.ConcluidaDe is null ||
                        aceite.AceitoEmUtc >= InicioUtc(
                            filtro.ConcluidaDe.Value)) &&
                    (filtro.ConcluidaAte is null ||
                        filtro.ConcluidaAte == DateOnly.MaxValue ||
                        aceite.AceitoEmUtc < InicioUtc(
                            filtro.ConcluidaAte.Value.AddDays(1))))
                .Select(aceite => aceite.FichaId)
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
        var consultaOrdenada = providerIsSqlite
            ? consulta.OrderByDescending(ficha => ficha.Id)
            : consulta
                .OrderByDescending(ficha => ficha.ConcluidaEmUtc)
                .ThenByDescending(ficha => ficha.CriadaEmUtc)
                .ThenBy(ficha => ficha.Id);
        var registros = await consultaOrdenada
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
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

    public async Task<DetalheFichaConsultada?> ObterDetalheAsync(
        Guid fichaId,
        CancellationToken cancellationToken)
    {
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == fichaId, cancellationToken);

        if (ficha is null)
        {
            return null;
        }

        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleAsync(item => item.Id == ficha.ClienteId, cancellationToken);
        var dadosDaFicha = await dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var questionario = await dbContext.QuestionariosSaude
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var aceite = await dbContext.AceitesTermoConsentimento
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var conviteExpiraEmUtc = await dbContext.ConvitesFicha
            .AsNoTracking()
            .Where(convite => convite.FichaId == ficha.Id)
            .Select(convite => (DateTimeOffset?)convite.ExpiraEmUtc)
            .SingleOrDefaultAsync(cancellationToken);
        var instanteAtual = timeProvider.GetUtcNow();

        return new DetalheFichaConsultada(
            ficha.Id,
            ficha.Status.ToString(),
            ficha.CriadaEmUtc,
            conviteExpiraEmUtc,
            conviteExpiraEmUtc is not null &&
                conviteExpiraEmUtc <= instanteAtual,
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.TipoProcedimento.ToString(),
            new ClienteDaFichaConsultado(
                cliente.Id,
                cliente.NomeReferencia,
                dadosDaFicha?.NomeCompleto,
                dadosDaFicha?.NomeSocial,
                dadosDaFicha?.NomeParaExibicao ?? cliente.NomeReferencia,
                dadosDaFicha?.Pronomes,
                dadosDaFicha?.DataNascimento,
                dadosDaFicha?.Celular,
                dadosDaFicha?.Email,
                dadosDaFicha?.Instagram,
                dadosDaFicha?.ContatoEmergenciaNome,
                dadosDaFicha?.ContatoEmergenciaCelular,
                dadosDaFicha?.ConfirmadosEmUtc),
            questionario is null
                ? null
                : new QuestionarioSaudeConsultado(
                    questionario.Versao,
                    questionario.TemDiabetes,
                    questionario.TipoDiabetes,
                    questionario.PossuiPressaoAlta,
                    questionario.TemAlergia,
                    questionario.DescricaoAlergia,
                    questionario.PossuiCondicaoCardiaca,
                    questionario.TemEpilepsia,
                    questionario.TemHemofilia,
                    questionario.UsaMarcaPasso,
                    questionario.EstaGravidaOuAmamentando,
                    questionario.RespondidoEmUtc),
            aceite is null
                ? null
                : new AceiteTermoConsultado(
                    aceite.VersaoTermo,
                    aceite.NomeAssinante,
                    aceite.AceitoEmUtc,
                    aceite.ConfirmouMaioridade,
                    aceite.ConfirmouDadosPessoais,
                    aceite.ConfirmouQuestionarioSaude,
                    aceite.EvidenciaHash,
                    string.Equals(
                        calculadorHash.Calcular(aceite.EvidenciaJson),
                        aceite.EvidenciaHash,
                        StringComparison.OrdinalIgnoreCase)));
    }

    private static DateTimeOffset InicioUtc(DateOnly data)
    {
        return new DateTimeOffset(
            data.ToDateTime(TimeOnly.MinValue),
            TimeSpan.Zero);
    }
}

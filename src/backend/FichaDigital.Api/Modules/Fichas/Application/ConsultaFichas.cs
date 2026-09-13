using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record FiltroConsultaFichas(
    string? Busca,
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
    string? EstadoCivil,
    DateOnly? DataNascimento,
    string? Cpf,
    string? Celular,
    string? TelefoneAdicional,
    string? Email,
    string? Instagram,
    string? ContatoEmergenciaNome,
    string? ContatoEmergenciaCelular,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    DateTimeOffset? DadosPessoaisPreenchidosEmUtc);

public sealed record QuestionarioSaudeConsultado(
    int Versao,
    bool TemDiabetes,
    string? TipoDiabetes,
    bool TeveAnemia,
    string? DescricaoAnemia,
    bool TeveHepatite,
    string? TipoHepatite,
    bool PossuiPressaoAlta,
    bool TemAlergia,
    string? DescricaoAlergia,
    bool PossuiCondicaoCardiaca,
    bool TemEpilepsia,
    bool TemHemofilia,
    bool PossuiDoencaTransmissivel,
    string? DescricaoDoencaTransmissivel,
    bool UsaMarcaPasso,
    bool Fuma,
    bool ConsumiuBebidaAlcoolicaUltimas24Horas,
    bool UsaMedicacao,
    string? DescricaoMedicacao,
    bool EstaGravidaOuAmamentando,
    DateTimeOffset RespondidoEmUtc);

public sealed record AceiteTermoConsultado(
    int VersaoTermo,
    string NomeAssinante,
    DateTimeOffset AceitoEmUtc,
    bool ConfirmouLeituraEAutorizacao,
    string? AssinaturaDesenhada,
    string EvidenciaHash,
    bool EvidenciaIntegra);

public sealed record RevisaoProfissionalConsultada(
    string ProfissionalNome,
    bool DadosDaFichaConferidos,
    DateTimeOffset RevisadaEmUtc);

public sealed record RegistroProcedimentoConsultado(
    string TipoProcedimento,
    string ProfissionalNome,
    string? ArteEfetivamenteTatuada,
    string? MaterialUtilizado,
    string? LocalTatuagem,
    string? JoiaUtilizada,
    string? AgulhaUtilizada,
    string? LocalPerfuracao,
    string? Observacoes,
    decimal ValorTotal,
    decimal ValorSinal,
    string FormaPagamento,
    string NomeProfissionalAssinante,
    string AssinaturaDesenhada,
    DateTimeOffset RegistradoEmUtc,
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
    int? VersaoModelo,
    int? VersaoQuestionario,
    int? VersaoTermo,
    string? CnpjApresentado,
    ClienteDaFichaConsultado Cliente,
    QuestionarioSaudeConsultado? QuestionarioSaude,
    AceiteTermoConsultado? AceiteTermo,
    RevisaoProfissionalConsultada? RevisaoProfissional,
    RegistroProcedimentoConsultado? RegistroProcedimento);

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
        var revisao = await dbContext.RevisoesProfissionais
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var registroTatuagem = await dbContext.RegistrosTatuagem
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var registroPiercing = await dbContext.RegistrosPiercing
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
            ficha.VersaoModelo,
            ficha.VersaoQuestionario,
            ficha.VersaoTermo,
            ficha.CnpjApresentado,
            new ClienteDaFichaConsultado(
                cliente.Id,
                cliente.NomeReferencia,
                dadosDaFicha?.NomeCompleto,
                dadosDaFicha?.NomeSocial,
                dadosDaFicha?.NomeParaExibicao ?? cliente.NomeReferencia,
                dadosDaFicha?.Pronomes,
                dadosDaFicha?.EstadoCivil,
                dadosDaFicha?.DataNascimento,
                dadosDaFicha?.Cpf,
                dadosDaFicha?.Celular,
                dadosDaFicha?.TelefoneAdicional,
                dadosDaFicha?.Email,
                dadosDaFicha?.Instagram,
                dadosDaFicha?.ContatoEmergenciaNome,
                dadosDaFicha?.ContatoEmergenciaCelular,
                dadosDaFicha?.Cep,
                dadosDaFicha?.Logradouro,
                dadosDaFicha?.Numero,
                dadosDaFicha?.Complemento,
                dadosDaFicha?.Bairro,
                dadosDaFicha?.Cidade,
                dadosDaFicha?.Estado,
                dadosDaFicha?.ConfirmadosEmUtc),
            questionario is null
                ? null
                : new QuestionarioSaudeConsultado(
                    questionario.Versao,
                    questionario.TemDiabetes,
                    questionario.TipoDiabetes,
                    questionario.TeveAnemia,
                    questionario.DescricaoAnemia,
                    questionario.TeveHepatite,
                    questionario.TipoHepatite,
                    questionario.PossuiPressaoAlta,
                    questionario.TemAlergia,
                    questionario.DescricaoAlergia,
                    questionario.PossuiCondicaoCardiaca,
                    questionario.TemEpilepsia,
                    questionario.TemHemofilia,
                    questionario.PossuiDoencaTransmissivel,
                    questionario.DescricaoDoencaTransmissivel,
                    questionario.UsaMarcaPasso,
                    questionario.Fuma,
                    questionario.ConsumiuBebidaAlcoolicaUltimas24Horas,
                    questionario.UsaMedicacao,
                    questionario.DescricaoMedicacao,
                    questionario.EstaGravidaOuAmamentando,
                    questionario.RespondidoEmUtc),
            aceite is null
                ? null
                : new AceiteTermoConsultado(
                    aceite.VersaoTermo,
                    aceite.NomeAssinante,
                    aceite.AceitoEmUtc,
                    aceite.ConfirmouLeituraEAutorizacao,
                    aceite.AssinaturaDesenhada,
                    aceite.EvidenciaHash,
                    string.Equals(
                        calculadorHash.Calcular(aceite.EvidenciaJson),
                        aceite.EvidenciaHash,
                        StringComparison.OrdinalIgnoreCase)),
            revisao is null
                ? null
                : new RevisaoProfissionalConsultada(
                    revisao.ProfissionalNome,
                    revisao.DadosDaFichaConferidos,
                    revisao.RevisadaEmUtc),
            CriarRegistroConsultado(registroTatuagem, registroPiercing));
    }

    private RegistroProcedimentoConsultado? CriarRegistroConsultado(
        RegistroTatuagem? tatuagem,
        RegistroPiercing? piercing)
    {
        if (tatuagem is not null)
        {
            return new RegistroProcedimentoConsultado(
                TipoProcedimento.Tatuagem.ToString(),
                tatuagem.ProfissionalNome,
                tatuagem.ArteEfetivamenteTatuada,
                tatuagem.MaterialUtilizado,
                tatuagem.LocalTatuagem,
                null,
                null,
                null,
                tatuagem.Observacoes,
                tatuagem.ValorTotal,
                tatuagem.ValorSinal,
                tatuagem.FormaPagamento.ToString(),
                tatuagem.NomeProfissionalAssinante,
                tatuagem.AssinaturaDesenhada,
                tatuagem.RegistradoEmUtc,
                tatuagem.EvidenciaHash,
                string.Equals(
                    calculadorHash.Calcular(tatuagem.EvidenciaJson),
                    tatuagem.EvidenciaHash,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (piercing is not null)
        {
            return new RegistroProcedimentoConsultado(
                TipoProcedimento.Piercing.ToString(),
                piercing.ProfissionalNome,
                null,
                null,
                null,
                piercing.JoiaUtilizada,
                piercing.AgulhaUtilizada,
                piercing.LocalPerfuracao,
                piercing.Observacoes,
                piercing.ValorTotal,
                piercing.ValorSinal,
                piercing.FormaPagamento.ToString(),
                piercing.NomeProfissionalAssinante,
                piercing.AssinaturaDesenhada,
                piercing.RegistradoEmUtc,
                piercing.EvidenciaHash,
                string.Equals(
                    calculadorHash.Calcular(piercing.EvidenciaJson),
                    piercing.EvidenciaHash,
                    StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    private static DateTimeOffset InicioUtc(DateOnly data)
    {
        return HorarioEstudio.ObterInicioUtc(data);
    }
}

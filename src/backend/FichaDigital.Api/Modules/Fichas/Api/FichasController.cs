using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Atendimentos.Api;
using FichaDigital.Api.Modules.Atendimentos.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/fichas")]
public sealed class FichasController(
    FichaDigitalDbContext dbContext,
    TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<FichasPaginadasResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FichasPaginadasResponse>> Listar(
        [FromQuery] ListarFichasRequest request,
        CancellationToken cancellationToken)
    {
        var providerIsSqlite = string.Equals(
            dbContext.Database.ProviderName,
            "Microsoft.EntityFrameworkCore.Sqlite",
            StringComparison.Ordinal);
        var consulta =
            from ficha in dbContext.Fichas.AsNoTracking()
            join cliente in dbContext.Clientes.AsNoTracking()
                on ficha.ClienteId equals cliente.Id
            join atendimento in dbContext.Atendimentos.AsNoTracking()
                on ficha.Id equals atendimento.FichaId into atendimentos
            from atendimento in atendimentos.DefaultIfEmpty()
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
                ClienteNome = cliente.NomeSocial ??
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
                AtendimentoId = atendimento == null
                    ? null
                    : (Guid?)atendimento.Id,
                AtendimentoDataRealizacao = atendimento == null
                    ? null
                    : (DateOnly?)atendimento.DataRealizacao,
                AtendimentoValorCobrado = atendimento == null
                    ? null
                    : (decimal?)atendimento.ValorCobrado,
                AtendimentoDesconto = atendimento == null
                    ? null
                    : (decimal?)atendimento.Desconto,
                AtendimentoValorFinal = atendimento == null
                    ? null
                    : (decimal?)atendimento.ValorFinal,
                AtendimentoFormaPagamento = atendimento == null
                    ? null
                    : (FormaPagamento?)atendimento.FormaPagamento,
                AtendimentoSituacaoPagamento = atendimento == null
                    ? null
                    : (SituacaoPagamento?)atendimento.SituacaoPagamento,
                AtendimentoRegistradoEmUtc = atendimento == null
                    ? null
                    : (DateTimeOffset?)atendimento.RegistradoEmUtc,
                AtendimentoAtualizadoEmUtc = atendimento == null
                    ? null
                    : (DateTimeOffset?)atendimento.AtualizadoEmUtc,
                ConviteExpiraEmUtc = dbContext.ConvitesFicha
                    .Where(convite => convite.FichaId == ficha.Id)
                    .Select(convite => (DateTimeOffset?)convite.ExpiraEmUtc)
                    .FirstOrDefault()
            };

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.Trim();
            consulta = consulta.Where(ficha =>
                ficha.ClienteNome.Contains(busca) ||
                ficha.NomeReferencia.Contains(busca) ||
                (ficha.NomeCompleto != null &&
                 ficha.NomeCompleto.Contains(busca)) ||
                (ficha.NomeSocial != null &&
                 ficha.NomeSocial.Contains(busca)) ||
                (ficha.Email != null && ficha.Email.Contains(busca)) ||
                (ficha.Celular != null && ficha.Celular.Contains(busca)) ||
                ficha.ProfissionalResponsavelNome.Contains(busca));
        }

        if (request.ProfissionalId is not null)
        {
            consulta = consulta.Where(ficha =>
                ficha.ProfissionalResponsavelId == request.ProfissionalId);
        }

        if (request.TipoProcedimento is not null)
        {
            consulta = consulta.Where(ficha =>
                ficha.TipoProcedimento == request.TipoProcedimento);
        }

        if (request.Status is not null)
        {
            consulta = consulta.Where(ficha =>
                ficha.Status == request.Status);
        }

        if (request.CriadaDe is not null)
        {
            var inicio = new DateTimeOffset(
                request.CriadaDe.Value.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);
            consulta = consulta.Where(ficha =>
                ficha.CriadaEmUtc >= inicio);
        }

        if (request.CriadaAte is not null &&
            request.CriadaAte.Value != DateOnly.MaxValue)
        {
            var fimExclusivo = new DateTimeOffset(
                request.CriadaAte.Value
                    .AddDays(1)
                    .ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);
            consulta = consulta.Where(ficha =>
                ficha.CriadaEmUtc < fimExclusivo);
        }

        if (providerIsSqlite &&
            (request.AtendimentoDe is not null ||
             request.AtendimentoAte is not null))
        {
            // SQLite não traduz comparações combinadas de DateOnly e
            // DateTimeOffset. Ele é usado apenas nos testes, então calculamos
            // os identificadores em memória para manter a mesma regra do SQL
            // Server: data do procedimento ou, enquanto não houver registro
            // financeiro, data de conclusão da ficha.
            var atendimentosRegistrados = await dbContext.Atendimentos
                .AsNoTracking()
                .Select(atendimento => new
                {
                    atendimento.FichaId,
                    atendimento.DataRealizacao
                })
                .ToListAsync(cancellationToken);
            var fichasComAtendimento = atendimentosRegistrados
                .Select(atendimento => atendimento.FichaId)
                .ToHashSet();
            var fichasNoPeriodo = atendimentosRegistrados
                .Where(atendimento =>
                    (request.AtendimentoDe is null ||
                     atendimento.DataRealizacao >= request.AtendimentoDe) &&
                    (request.AtendimentoAte is null ||
                     atendimento.DataRealizacao <= request.AtendimentoAte))
                .Select(atendimento => atendimento.FichaId)
                .ToHashSet();
            var conclusoes = await dbContext.AceitesTermoConsentimento
                .AsNoTracking()
                .Select(aceite => new
                {
                    aceite.FichaId,
                    aceite.AceitoEmUtc
                })
                .ToListAsync(cancellationToken);

            foreach (var conclusao in conclusoes)
            {
                if (fichasComAtendimento.Contains(conclusao.FichaId))
                {
                    continue;
                }

                var dataConclusao = DateOnly.FromDateTime(
                    conclusao.AceitoEmUtc.UtcDateTime);
                if ((request.AtendimentoDe is null ||
                     dataConclusao >= request.AtendimentoDe) &&
                    (request.AtendimentoAte is null ||
                     dataConclusao <= request.AtendimentoAte))
                {
                    fichasNoPeriodo.Add(conclusao.FichaId);
                }
            }

            consulta = consulta.Where(ficha =>
                fichasNoPeriodo.Contains(ficha.Id));
        }
        else if (request.AtendimentoDe is not null)
        {
            var inicio = new DateTimeOffset(
                request.AtendimentoDe.Value.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);
            consulta = consulta.Where(ficha =>
                (ficha.AtendimentoDataRealizacao != null &&
                 ficha.AtendimentoDataRealizacao >= request.AtendimentoDe) ||
                (ficha.AtendimentoDataRealizacao == null &&
                 ficha.ConcluidaEmUtc >= inicio));
        }

        if (!providerIsSqlite &&
            request.AtendimentoAte is not null &&
            request.AtendimentoAte != DateOnly.MaxValue)
        {
            var fimExclusivo = new DateTimeOffset(
                request.AtendimentoAte.Value
                    .AddDays(1)
                    .ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);
            consulta = consulta.Where(ficha =>
                (ficha.AtendimentoDataRealizacao != null &&
                 ficha.AtendimentoDataRealizacao <= request.AtendimentoAte) ||
                (ficha.AtendimentoDataRealizacao == null &&
                 ficha.ConcluidaEmUtc < fimExclusivo));
        }

        var totalItens = await consulta.CountAsync(cancellationToken);
        var valoresAtendimentos = await consulta
            .Where(ficha => ficha.AtendimentoId != null)
            .Select(ficha => new
            {
                ValorFinal = ficha.AtendimentoValorFinal!.Value,
                SituacaoPagamento =
                    ficha.AtendimentoSituacaoPagamento!.Value
            })
            .ToListAsync(cancellationToken);
        var resumoFinanceiro = new ResumoFinanceiroFichasResponse(
            valoresAtendimentos.Sum(item => item.ValorFinal),
            valoresAtendimentos.Count,
            totalItens - valoresAtendimentos.Count);
        var totalPaginas = (int)Math.Ceiling(
            totalItens / (double)request.TamanhoPagina);
        var consultaOrdenada = providerIsSqlite
            // SQLite é usado somente nos testes e não ordena DateTimeOffset.
            ? consulta.OrderByDescending(ficha => ficha.Id)
            : consulta
                .OrderByDescending(ficha => ficha.AtendimentoDataRealizacao)
                .ThenByDescending(ficha => ficha.ConcluidaEmUtc)
                .ThenByDescending(ficha => ficha.CriadaEmUtc);
        var registros = await consultaOrdenada
            .ThenBy(ficha => ficha.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .ToListAsync(cancellationToken);
        var instanteAtual = timeProvider.GetUtcNow();
        var itens = registros
            .Select(ficha => new FichaResumoResponse(
                ficha.Id,
                ficha.ClienteId,
                ficha.ClienteNome,
                ficha.ProfissionalResponsavelId,
                ficha.ProfissionalResponsavelNome,
                ficha.TipoProcedimento.ToString(),
                ficha.Status.ToString(),
                ficha.CriadaEmUtc,
                ficha.ConcluidaEmUtc,
                ficha.AtendimentoId is null
                    ? null
                    : new AtendimentoResponse(
                        ficha.AtendimentoId.Value,
                        ficha.Id,
                        ficha.AtendimentoDataRealizacao!.Value,
                        ficha.AtendimentoValorCobrado!.Value,
                        ficha.AtendimentoDesconto!.Value,
                        ficha.AtendimentoValorFinal!.Value,
                        ficha.AtendimentoFormaPagamento!.Value.ToString(),
                        ficha.AtendimentoSituacaoPagamento!.Value.ToString(),
                        ficha.AtendimentoRegistradoEmUtc!.Value,
                        ficha.AtendimentoAtualizadoEmUtc!.Value),
                ficha.ConviteExpiraEmUtc,
                ficha.ConviteExpiraEmUtc is not null &&
                    ficha.ConviteExpiraEmUtc <= instanteAtual))
            .ToList();

        return Ok(new FichasPaginadasResponse(
            itens,
            request.Pagina,
            request.TamanhoPagina,
            totalItens,
            totalPaginas,
            resumoFinanceiro));
    }

    [HttpGet("{fichaId:guid}")]
    [ProducesResponseType<FichaDetalheResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FichaDetalheResponse>> ObterDetalhe(
        Guid fichaId,
        CancellationToken cancellationToken)
    {
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == fichaId,
                cancellationToken);

        if (ficha is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Ficha não encontrada.",
                detail: "Não existe uma ficha com o identificador informado.");
        }

        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == ficha.ClienteId,
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
        var atendimento = await dbContext.Atendimentos
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

        var response = new FichaDetalheResponse(
            ficha.Id,
            ficha.Status.ToString(),
            ficha.CriadaEmUtc,
            conviteExpiraEmUtc,
            conviteExpiraEmUtc is not null &&
                conviteExpiraEmUtc <= instanteAtual,
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.TipoProcedimento.ToString(),
            new ClienteFichaDetalheResponse(
                cliente.Id,
                cliente.NomeReferencia,
                cliente.NomeCompleto,
                cliente.NomeSocial,
                cliente.NomeSocial ??
                    cliente.NomeCompleto ??
                    cliente.NomeReferencia,
                cliente.Pronomes,
                cliente.DataNascimento,
                cliente.Celular,
                cliente.Email,
                cliente.Instagram,
                cliente.ContatoEmergenciaNome,
                cliente.ContatoEmergenciaCelular,
                cliente.DadosPessoaisPreenchidosEmUtc),
            questionario is null
                ? null
                : new QuestionarioSaudeDetalheResponse(
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
                : new AceiteTermoResumoResponse(
                    aceite.VersaoTermo,
                    aceite.NomeAssinante,
                    aceite.AceitoEmUtc),
            atendimento is null
                ? null
                : new AtendimentoResponse(
                    atendimento.Id,
                    atendimento.FichaId,
                    atendimento.DataRealizacao,
                    atendimento.ValorCobrado,
                    atendimento.Desconto,
                    atendimento.ValorFinal,
                    atendimento.FormaPagamento.ToString(),
                    atendimento.SituacaoPagamento.ToString(),
                    atendimento.RegistradoEmUtc,
                    atendimento.AtualizadoEmUtc));

        return Ok(response);
    }
}

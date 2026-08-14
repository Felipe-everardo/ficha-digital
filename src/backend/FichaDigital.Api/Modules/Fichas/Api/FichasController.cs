using FichaDigital.Api.Infrastructure.Persistence;
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
        var consulta =
            from ficha in dbContext.Fichas.AsNoTracking()
            join cliente in dbContext.Clientes.AsNoTracking()
                on ficha.ClienteId equals cliente.Id
            select new
            {
                ficha.Id,
                ficha.ClienteId,
                ClienteNome = cliente.NomeSocial ?? cliente.NomeCompleto,
                ficha.Status,
                ficha.CriadaEmUtc,
                ConviteExpiraEmUtc = dbContext.ConvitesFicha
                    .Where(convite => convite.FichaId == ficha.Id)
                    .Select(convite => (DateTimeOffset?)convite.ExpiraEmUtc)
                    .FirstOrDefault()
            };
        var totalItens = await consulta.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(
            totalItens / (double)request.TamanhoPagina);
        var consultaOrdenada = string.Equals(
                dbContext.Database.ProviderName,
                "Microsoft.EntityFrameworkCore.Sqlite",
                StringComparison.Ordinal)
            // SQLite é usado somente nos testes e não ordena DateTimeOffset.
            ? consulta.OrderByDescending(ficha => ficha.Id)
            : consulta.OrderByDescending(ficha => ficha.CriadaEmUtc);
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
                ficha.Status.ToString(),
                ficha.CriadaEmUtc,
                ficha.ConviteExpiraEmUtc,
                ficha.ConviteExpiraEmUtc is not null &&
                    ficha.ConviteExpiraEmUtc <= instanteAtual))
            .ToList();

        return Ok(new FichasPaginadasResponse(
            itens,
            request.Pagina,
            request.TamanhoPagina,
            totalItens,
            totalPaginas));
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
            new ClienteFichaDetalheResponse(
                cliente.Id,
                cliente.NomeCompleto,
                cliente.NomeSocial,
                cliente.NomeSocial ?? cliente.NomeCompleto,
                cliente.Pronomes,
                cliente.DataNascimento,
                cliente.Celular,
                cliente.Email),
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
                    aceite.AceitoEmUtc));

        return Ok(response);
    }
}

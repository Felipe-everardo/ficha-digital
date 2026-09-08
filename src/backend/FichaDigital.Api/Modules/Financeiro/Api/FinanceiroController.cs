using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Financeiro.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Financeiro.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/financeiro")]
public sealed class FinanceiroController(
    FichaDigitalDbContext dbContext,
    UserManager<ProfissionalUsuario> userManager,
    TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<FinanceiroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FinanceiroResponse>> Listar(
        [FromQuery] ListarFinanceiroRequest request,
        CancellationToken cancellationToken)
    {
        var atendimentos = dbContext.Atendimentos.AsNoTracking();
        var despesas = dbContext.Despesas.AsNoTracking();

        if (request.DataDe is not null)
        {
            atendimentos = atendimentos.Where(item =>
                item.DataRealizacao >= request.DataDe);
            despesas = despesas.Where(item => item.Data >= request.DataDe);
        }

        if (request.DataAte is not null)
        {
            atendimentos = atendimentos.Where(item =>
                item.DataRealizacao <= request.DataAte);
            despesas = despesas.Where(item => item.Data <= request.DataAte);
        }

        // Os valores são somados em memória porque o SQLite dos testes não
        // oferece suporte consistente a agregações sobre decimal.
        var valoresAtendimentos = await atendimentos
            .Select(item => item.ValorFinal)
            .ToListAsync(cancellationToken);
        var valoresDespesas = await despesas
            .Select(item => item.Valor)
            .ToListAsync(cancellationToken);

        var totalRecebido = valoresAtendimentos.Sum();
        var totalSaidas = valoresDespesas.Sum();
        var totalDespesas = valoresDespesas.Count;
        var totalPaginas = (int)Math.Ceiling(
            totalDespesas / (double)request.TamanhoPagina);

        var registros = await despesas
            .OrderByDescending(item => item.Data)
            .ThenByDescending(item => item.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .ToListAsync(cancellationToken);

        var resumo = new ResumoFinanceiroResponse(
            totalRecebido,
            totalSaidas,
            totalRecebido - totalSaidas,
            valoresAtendimentos.Count);

        return Ok(new FinanceiroResponse(
            resumo,
            registros.Select(CriarResponse).ToList(),
            request.Pagina,
            request.TamanhoPagina,
            totalDespesas,
            totalPaginas));
    }

    [HttpPost("despesas")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<DespesaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DespesaResponse>> CriarDespesa(
        [FromBody] SalvarDespesaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ValidarData(request.Data))
        {
            return ValidationProblem(ModelState);
        }

        var profissional = await userManager.GetUserAsync(User);
        if (profissional is null)
        {
            return Unauthorized();
        }

        var instanteAtual = timeProvider.GetUtcNow();
        var despesa = new Despesa(
            request.Data!.Value,
            request.Categoria!.Value,
            request.Descricao,
            request.Valor,
            profissional.Id,
            profissional.NomeCompleto,
            instanteAtual);

        dbContext.Despesas.Add(despesa);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/financeiro/despesas/{despesa.Id}",
            CriarResponse(despesa));
    }

    [HttpPut("despesas/{despesaId:guid}")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<DespesaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DespesaResponse>> AtualizarDespesa(
        Guid despesaId,
        [FromBody] SalvarDespesaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ValidarData(request.Data))
        {
            return ValidationProblem(ModelState);
        }

        var despesa = await dbContext.Despesas.SingleOrDefaultAsync(
            item => item.Id == despesaId,
            cancellationToken);
        if (despesa is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Despesa não encontrada.",
                detail: "Não existe uma despesa com o identificador informado.");
        }

        despesa.Atualizar(
            request.Data!.Value,
            request.Categoria!.Value,
            request.Descricao,
            request.Valor,
            timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(CriarResponse(despesa));
    }

    private bool ValidarData(DateOnly? data)
    {
        if (data is null)
        {
            return true;
        }

        var hojeUtc = DateOnly.FromDateTime(
            timeProvider.GetUtcNow().UtcDateTime);
        if (data <= hojeUtc)
        {
            return true;
        }

        ModelState.AddModelError(
            nameof(SalvarDespesaRequest.Data),
            "A data da despesa não pode estar no futuro.");
        return false;
    }

    private static DespesaResponse CriarResponse(Despesa despesa)
    {
        return new DespesaResponse(
            despesa.Id,
            despesa.Data,
            despesa.Categoria.ToString(),
            despesa.Descricao,
            despesa.Valor,
            despesa.ProfissionalId,
            despesa.ProfissionalNome,
            despesa.RegistradaEmUtc,
            despesa.AtualizadaEmUtc);
    }
}

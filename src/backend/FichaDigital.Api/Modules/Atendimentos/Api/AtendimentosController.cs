using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Atendimentos.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Atendimentos.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/fichas/{fichaId:guid}/atendimento")]
public sealed class AtendimentosController(
    FichaDigitalDbContext dbContext,
    TimeProvider timeProvider) : ControllerBase
{
    [HttpPut]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<AtendimentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<AtendimentoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AtendimentoResponse>> Registrar(
        Guid fichaId,
        [FromBody] RegistrarAtendimentoRequest request,
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

        if (ficha.Status != StatusFicha.Concluida)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ficha ainda não concluída.",
                detail: "O atendimento só pode ser registrado após o cliente concluir a ficha.");
        }

        var hojeUtc = DateOnly.FromDateTime(
            timeProvider.GetUtcNow().UtcDateTime);
        if (request.DataRealizacao > hojeUtc)
        {
            ModelState.AddModelError(
                nameof(request.DataRealizacao),
                "A data do procedimento não pode estar no futuro.");
            return ValidationProblem(ModelState);
        }

        var instanteAtual = timeProvider.GetUtcNow();
        var atendimento = await dbContext.Atendimentos
            .SingleOrDefaultAsync(
                item => item.FichaId == fichaId,
                cancellationToken);
        var novoAtendimento = atendimento is null;

        if (atendimento is null)
        {
            atendimento = new Atendimento(
                fichaId,
                request.DataRealizacao!.Value,
                request.ValorCobrado,
                request.Desconto,
                request.FormaPagamento!.Value,
                instanteAtual);
            dbContext.Atendimentos.Add(atendimento);
        }
        else
        {
            atendimento.Atualizar(
                request.DataRealizacao!.Value,
                request.ValorCobrado,
                request.Desconto,
                request.FormaPagamento!.Value,
                instanteAtual);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = CriarResponse(atendimento);
        return novoAtendimento
            ? Created($"/api/fichas/{fichaId}", response)
            : Ok(response);
    }

    private static AtendimentoResponse CriarResponse(Atendimento atendimento)
    {
        return new AtendimentoResponse(
            atendimento.Id,
            atendimento.FichaId,
            atendimento.DataRealizacao,
            atendimento.ValorCobrado,
            atendimento.Desconto,
            atendimento.ValorFinal,
            atendimento.FormaPagamento.ToString(),
            atendimento.SituacaoPagamento.ToString(),
            atendimento.RegistradoEmUtc,
            atendimento.AtualizadoEmUtc);
    }
}

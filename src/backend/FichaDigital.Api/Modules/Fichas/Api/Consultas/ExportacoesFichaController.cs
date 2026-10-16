using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/fichas/{fichaId:guid}/exportacao-pdf")]
public sealed class ExportacoesFichaController(
    IConsultaDetalheFicha consulta,
    FichaDigitalDbContext dbContext,
    TimeProvider timeProvider) : ControllerBase
{
    // O middleware de auditoria registra profissional, ficha e instante desta solicitação.
    [HttpGet]
    public async Task<ActionResult<ExportacaoFichaResponse>> Obter(
        Guid fichaId, CancellationToken cancellationToken)
    {
        var ficha = await consulta.ObterAsync(fichaId, cancellationToken);
        if (ficha is null)
            return Problem(statusCode: 404, title: "Ficha não encontrada.");
        if (ficha.Status != "Concluida")
            return Problem(statusCode: 409, title: "Conclua a ficha antes de exportar.");

        var conteudo = await dbContext.AceitesTermoConsentimento.AsNoTracking()
            .Where(item => item.FichaId == fichaId)
            .Select(item => item.ConteudoTermo)
            .SingleOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(conteudo) || ficha.QuestionarioSaude is null)
            return Problem(statusCode: 409,
                title: "Esta ficha não possui todos os registros necessários para exportação.");
        if (ficha.AceiteTermo?.EvidenciaIntegra != true ||
            ficha.RegistroProcedimento is { EvidenciaIntegra: false })
            return Problem(statusCode: 409,
                title: "Não foi possível verificar a integridade da ficha para exportação.");

        return Ok(new ExportacaoFichaResponse(
            ficha.ToResponse(), conteudo, timeProvider.GetUtcNow()));
    }
}

public sealed record ExportacaoFichaResponse(
    FichaDetalheResponse Ficha, string ConteudoTermo, DateTimeOffset ExportadaEmUtc);

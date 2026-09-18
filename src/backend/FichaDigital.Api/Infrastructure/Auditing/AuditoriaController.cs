using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Auditing;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/auditoria")]
public sealed class AuditoriaController(
    Persistence.FichaDigitalDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegistroAuditoriaResponse>>>
        Listar(
            [FromQuery] string? recurso,
            [FromQuery] Guid? recursoId,
            [FromQuery] int limite = 100,
            CancellationToken cancellationToken = default)
    {
        limite = Math.Clamp(limite, 1, 200);
        var consulta = dbContext.RegistrosAuditoria.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(recurso))
        {
            var recursoNormalizado = recurso.Trim();
            consulta = consulta.Where(
                registro => registro.Recurso == recursoNormalizado);
        }

        if (recursoId is not null)
        {
            consulta = consulta.Where(
                registro => registro.RecursoId == recursoId);
        }

        var registros = await consulta
            .OrderByDescending(registro => registro.OcorreuEmUtc)
            .ThenByDescending(registro => registro.Id)
            .Take(limite)
            .Select(registro => new RegistroAuditoriaResponse(
                registro.Id,
                registro.ProfissionalId,
                registro.ProfissionalId == null
                    ? "Cliente via convite"
                    : dbContext.Users
                        .Where(usuario =>
                            usuario.Id == registro.ProfissionalId)
                        .Select(usuario => usuario.NomeCompleto)
                        .SingleOrDefault() ?? "Conta removida",
                registro.Origem,
                registro.Acao,
                registro.Recurso,
                registro.RecursoId,
                registro.CorrelacaoId,
                registro.OcorreuEmUtc))
            .ToListAsync(cancellationToken);

        return Ok(registros);
    }
}

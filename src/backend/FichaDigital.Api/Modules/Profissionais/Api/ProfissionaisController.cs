using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Profissionais.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/profissionais")]
public sealed class ProfissionaisController(
    FichaDigitalDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProfissionalResumoResponse>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ProfissionalResumoResponse>>>
        Listar(CancellationToken cancellationToken)
    {
        var profissionais = await dbContext.Users
            .AsNoTracking()
            .OrderBy(profissional => profissional.NomeCompleto)
            .ThenBy(profissional => profissional.Id)
            .Select(profissional => new
            {
                profissional.Id,
                profissional.NomeCompleto,
                profissional.Especialidades
            })
            .ToListAsync(cancellationToken);

        return Ok(profissionais
            .Select(profissional => new ProfissionalResumoResponse(
                profissional.Id,
                profissional.NomeCompleto,
                ListarEspecialidades(profissional.Especialidades)))
            .ToList());
    }

    private static IReadOnlyList<string> ListarEspecialidades(
        EspecialidadesProfissional especialidades)
    {
        var resultado = new List<string>();

        if (especialidades.HasFlag(EspecialidadesProfissional.Tatuagem))
        {
            resultado.Add("Tatuagem");
        }

        if (especialidades.HasFlag(
                EspecialidadesProfissional.BodyPiercing))
        {
            resultado.Add("BodyPiercing");
        }

        return resultado;
    }
}

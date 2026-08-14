using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FichaDigital.Api.Modules.Profissionais.Api;

[ApiController]
[Route("api/autenticacao")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AutenticacaoProfissionaisController(
    UserManager<ProfissionalUsuario> userManager,
    SignInManager<ProfissionalUsuario> signInManager,
    IAntiforgery antiforgery) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("antiforgery-token")]
    [ProducesResponseType<AntiforgeryTokenResponse>(
        StatusCodes.Status200OK)]
    public ActionResult<AntiforgeryTokenResponse> ObterAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new AntiforgeryTokenResponse(
            tokens.RequestToken!));
    }

    [AllowAnonymous]
    [HttpPost("entrar")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(
        PoliticasRateLimitingAutenticacao.LoginProfissionais)]
    [ProducesResponseType<SessaoProfissionalResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SessaoProfissionalResponse>> Entrar(
        [FromBody] EntrarProfissionalRequest request)
    {
        var email = request.Email.Trim();
        var profissional = await userManager.FindByEmailAsync(email);

        if (profissional is null)
        {
            return CredenciaisInvalidas();
        }

        var resultado = await signInManager.PasswordSignInAsync(
            profissional,
            request.Senha,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!resultado.Succeeded)
        {
            return CredenciaisInvalidas();
        }

        return Ok(CriarSessaoResponse(profissional));
    }

    [Authorize]
    [HttpGet("sessao")]
    [ProducesResponseType<SessaoProfissionalResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessaoProfissionalResponse>> ObterSessao()
    {
        var profissional = await userManager.GetUserAsync(User);

        if (profissional is null)
        {
            return Unauthorized();
        }

        return Ok(CriarSessaoResponse(profissional));
    }

    [Authorize]
    [HttpPost("sair")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Sair()
    {
        await signInManager.SignOutAsync();

        return NoContent();
    }

    private UnauthorizedObjectResult CredenciaisInvalidas()
    {
        return Unauthorized(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Não foi possível entrar.",
            Detail = "Confira o e-mail e a senha informados."
        });
    }

    private static SessaoProfissionalResponse CriarSessaoResponse(
        ProfissionalUsuario profissional)
    {
        return new SessaoProfissionalResponse(
            profissional.Id,
            profissional.NomeCompleto,
            profissional.Email!);
    }
}

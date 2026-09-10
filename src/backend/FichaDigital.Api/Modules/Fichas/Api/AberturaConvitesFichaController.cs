using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[AllowAnonymous]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/fichas/convites")]
[EnableRateLimiting(PoliticasRateLimitingFichas.ConvitesPublicos)]
public sealed class AberturaConvitesFichaController(
    AbrirConviteFichaService service,
    CalculadorHashConteudo calculadorHash) : ControllerBase
{
    [HttpPost("abrir")]
    [ProducesResponseType<ConviteFichaAbertoResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status410Gone)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ConviteFichaAbertoResponse>> Abrir(
        [FromBody] AbrirConviteFichaRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await service.AbrirAsync(
            request.Token,
            cancellationToken);

        return resultado.Resultado switch
        {
            StatusAberturaConvite.Aberto => Ok(
                new ConviteFichaAbertoResponse(
                    resultado.FichaId!.Value,
                    resultado.StatusFicha!.Value.ToString(),
                    resultado.QuestionarioRespondido,
                    resultado.DadosPessoaisPreenchidos,
                    resultado.NomeReferencia!,
                    resultado.ProfissionalResponsavelNome!,
                    resultado.TipoProcedimento!.Value.ToString(),
                    new DadosPessoaisConviteResponse(
                        resultado.DadosPessoais!.NomeCompleto,
                        resultado.DadosPessoais.NomeSocial,
                        resultado.DadosPessoais.Pronomes,
                        resultado.DadosPessoais.DataNascimento,
                        resultado.DadosPessoais.Celular,
                        resultado.DadosPessoais.Email,
                        resultado.DadosPessoais.Instagram,
                        resultado.DadosPessoais.ContatoEmergenciaNome,
                        resultado.DadosPessoais.ContatoEmergenciaCelular),
                    resultado.QuestionarioSaude is null
                        ? null
                        : new QuestionarioSaudeDetalheResponse(
                            resultado.QuestionarioSaude.Versao,
                            resultado.QuestionarioSaude.TemDiabetes,
                            resultado.QuestionarioSaude.TipoDiabetes,
                            resultado.QuestionarioSaude.PossuiPressaoAlta,
                            resultado.QuestionarioSaude.TemAlergia,
                            resultado.QuestionarioSaude.DescricaoAlergia,
                            resultado.QuestionarioSaude.PossuiCondicaoCardiaca,
                            resultado.QuestionarioSaude.TemEpilepsia,
                            resultado.QuestionarioSaude.TemHemofilia,
                            resultado.QuestionarioSaude.UsaMarcaPasso,
                            resultado.QuestionarioSaude.EstaGravidaOuAmamentando,
                            resultado.QuestionarioSaude.RespondidoEmUtc),
                    new TermoConsentimentoResponse(
                        TermoConsentimentoAtual.Versao,
                        TermoConsentimentoAtual.Conteudo,
                        calculadorHash.Calcular(
                            TermoConsentimentoAtual.Conteudo)))),

            StatusAberturaConvite.Expirado => Problem(
                statusCode: StatusCodes.Status410Gone,
                title: "Convite expirado.",
                detail: "O prazo para utilizar este convite terminou."),

            StatusAberturaConvite.Indisponivel => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ficha indisponível.",
                detail: "Esta ficha não está disponível para preenchimento."),

            _ => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Convite inválido.",
                detail: "Não foi encontrado um convite válido para o token informado.")
        };
    }
}

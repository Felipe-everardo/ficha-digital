using FichaDigital.Api.Infrastructure.Auditing;
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
    CalculadorHashConteudo calculadorHash,
    AuditoriaService auditoriaService) : ControllerBase
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

        if (resultado.Resultado == StatusAberturaConvite.Aberto)
        {
            await auditoriaService.RegistrarAcaoDoClienteAsync(
                "Convite aberto",
                resultado.FichaId!.Value,
                HttpContext.TraceIdentifier,
                cancellationToken);
        }

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
                        resultado.DadosPessoais.EstadoCivil,
                        resultado.DadosPessoais.DataNascimento,
                        resultado.DadosPessoais.Cpf,
                        resultado.DadosPessoais.Celular,
                        resultado.DadosPessoais.TelefoneAdicional,
                        resultado.DadosPessoais.Email,
                        resultado.DadosPessoais.Instagram,
                        resultado.DadosPessoais.ContatoEmergenciaNome,
                        resultado.DadosPessoais.ContatoEmergenciaCelular,
                        resultado.DadosPessoais.Cep,
                        resultado.DadosPessoais.Logradouro,
                        resultado.DadosPessoais.Numero,
                        resultado.DadosPessoais.Complemento,
                        resultado.DadosPessoais.Bairro,
                        resultado.DadosPessoais.Cidade,
                        resultado.DadosPessoais.Estado),
                    resultado.QuestionarioSaude is null
                        ? null
                        : new QuestionarioSaudeDetalheResponse(
                            resultado.QuestionarioSaude.Versao,
                            resultado.QuestionarioSaude.TemDiabetes,
                            resultado.QuestionarioSaude.TipoDiabetes,
                            resultado.QuestionarioSaude.TeveAnemia,
                            resultado.QuestionarioSaude.DescricaoAnemia,
                            resultado.QuestionarioSaude.TeveHepatite,
                            resultado.QuestionarioSaude.TipoHepatite,
                            resultado.QuestionarioSaude.PossuiPressaoAlta,
                            resultado.QuestionarioSaude.TemAlergia,
                            resultado.QuestionarioSaude.DescricaoAlergia,
                            resultado.QuestionarioSaude.PossuiCondicaoCardiaca,
                            resultado.QuestionarioSaude.TemEpilepsia,
                            resultado.QuestionarioSaude.TemHemofilia,
                            resultado.QuestionarioSaude.PossuiDoencaTransmissivel,
                            resultado.QuestionarioSaude.DescricaoDoencaTransmissivel,
                            resultado.QuestionarioSaude.UsaMarcaPasso,
                            resultado.QuestionarioSaude.Fuma,
                            resultado.QuestionarioSaude.ConsumiuBebidaAlcoolicaUltimas24Horas,
                            resultado.QuestionarioSaude.UsaMedicacao,
                            resultado.QuestionarioSaude.DescricaoMedicacao,
                            resultado.QuestionarioSaude.EstaGravidaOuAmamentando,
                            resultado.QuestionarioSaude.RespondidoEmUtc),
                    new TermoConsentimentoResponse(
                        resultado.TermoConsentimento!.Versao,
                        resultado.TermoConsentimento.Conteudo,
                        calculadorHash.Calcular(
                            resultado.TermoConsentimento.Conteudo)))),

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

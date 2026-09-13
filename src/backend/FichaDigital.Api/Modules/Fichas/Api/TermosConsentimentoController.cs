using FichaDigital.Api.Infrastructure.Auditing;
using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[AllowAnonymous]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/fichas/termo-consentimento")]
[EnableRateLimiting(PoliticasRateLimitingFichas.ConvitesPublicos)]
public sealed class TermosConsentimentoController(
    AceitarTermoConsentimentoService service,
    AuditoriaService auditoriaService) : ControllerBase
{
    [HttpPost("aceitar")]
    [ProducesResponseType<TermoConsentimentoAceitoResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status410Gone)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TermoConsentimentoAceitoResponse>> Aceitar(
        [FromBody] AceitarTermoConsentimentoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AceitarTermoConsentimentoCommand(
            request.Token,
            request.VersaoTermo!.Value,
            request.ConteudoHash,
            request.NomeAssinante,
            request.ConfirmouLeituraEAutorizacao!.Value,
            LimitarMetadata(
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                64),
            LimitarMetadata(Request.Headers.UserAgent.ToString(), 512),
            request.AssinaturaDesenhada);

        var resultado = await service.AceitarAsync(
            command,
            cancellationToken);

        if (resultado.Resultado == StatusAceiteTermoConsentimento.Aceito)
        {
            await auditoriaService.RegistrarAcaoDoClienteAsync(
                "Consentimento assinado e procedimento autorizado",
                resultado.FichaId!.Value,
                HttpContext.TraceIdentifier,
                cancellationToken);
            var response = new TermoConsentimentoAceitoResponse(
                resultado.AceiteId!.Value,
                resultado.FichaId!.Value,
                resultado.VersaoTermo!.Value,
                resultado.AceitoEmUtc!.Value,
                resultado.EvidenciaHash!,
                StatusFicha.AutorizadaParaProcedimento.ToString());

            return Created(
                $"/api/fichas/{response.FichaId}/termo-consentimento",
                response);
        }

        return resultado.Resultado switch
        {
            StatusAceiteTermoConsentimento.ConviteExpirado => Problem(
                statusCode: StatusCodes.Status410Gone,
                title: "Convite expirado.",
                detail: "O prazo para concluir esta ficha terminou."),

            StatusAceiteTermoConsentimento.QuestionarioPendente => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Questionário pendente.",
                detail: "Responda o questionário antes de aceitar o termo."),

            StatusAceiteTermoConsentimento.DadosPessoaisPendentes => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Dados pessoais pendentes.",
                detail: "Confirme os dados pessoais antes de aceitar o termo."),

            StatusAceiteTermoConsentimento.TermoDesatualizado => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Termo atualizado.",
                detail: "O conteúdo do termo mudou. Recarregue a página e revise a versão atual."),

            StatusAceiteTermoConsentimento.JaAceito => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Termo já aceito.",
                detail: "Esta ficha já possui um aceite registrado."),

            StatusAceiteTermoConsentimento.FichaIndisponivel => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ficha indisponível.",
                detail: "Esta ficha não está disponível para conclusão."),

            StatusAceiteTermoConsentimento.ClienteMenorDeIdade => Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Atendimento indisponível.",
                detail: "O estúdio realiza procedimentos somente em pessoas com 18 anos ou mais."),

            StatusAceiteTermoConsentimento.ConfirmacaoObrigatoria => Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Confirmação obrigatória.",
                detail: "Confirme que leu, entendeu e autoriza o procedimento."),

            StatusAceiteTermoConsentimento.AssinaturaInvalida => Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Assinatura inválida.",
                detail: "Desenhe a assinatura antes de autorizar o procedimento."),

            StatusAceiteTermoConsentimento.NomeAssinanteDivergente => Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Nome divergente.",
                detail: "O nome digitado deve corresponder ao nome completo confirmado."),

            _ => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Convite inválido.",
                detail: "Não foi encontrado um convite válido para o token informado.")
        };
    }

    private static string? LimitarMetadata(string? valor, int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var normalizado = valor.Trim();
        return normalizado.Length <= tamanhoMaximo
            ? normalizado
            : normalizado[..tamanhoMaximo];
    }
}

using FichaDigital.Api.Modules.Fichas.Application;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Authorize]
[Route("api/fichas/{fichaId:guid}/operacoes")]
public sealed class OperacoesFichasController(
    RevisarFichaService revisarFichaService,
    ConcluirProcedimentoService concluirProcedimentoService,
    UserManager<ProfissionalUsuario> userManager) : ControllerBase
{
    [HttpPost("revisar")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<OperacaoFichaResponse>> Revisar(
        Guid fichaId,
        [FromBody] RevisarFichaRequest request,
        CancellationToken cancellationToken)
    {
        var profissional = await userManager.GetUserAsync(User);
        if (profissional is null)
        {
            return Unauthorized();
        }

        var resultado = await revisarFichaService.RevisarAsync(
            fichaId,
            profissional.Id,
            request.DadosDaFichaConferidos!.Value,
            cancellationToken);

        if (resultado.Resultado == StatusRevisaoFicha.Confirmada)
        {
            return Ok(new OperacaoFichaResponse(
                resultado.FichaId!.Value,
                resultado.StatusFicha!.Value.ToString()));
        }

        return CriarProblemaOperacao(
            resultado.Resultado == StatusRevisaoFicha.FichaNaoEncontrada,
            resultado.Resultado ==
                StatusRevisaoFicha.ProfissionalNaoResponsavel,
            resultado.Resultado == StatusRevisaoFicha.ConsentimentoInvalido
                ? "O consentimento não está íntegro para confirmar a revisão."
                : "A ficha não está pronta para a revisão profissional.");
    }

    [HttpPost("concluir")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<OperacaoFichaResponse>> Concluir(
        Guid fichaId,
        [FromBody] ConcluirProcedimentoRequest request,
        CancellationToken cancellationToken)
    {
        var profissional = await userManager.GetUserAsync(User);
        if (profissional is null)
        {
            return Unauthorized();
        }

        var resultado = await concluirProcedimentoService.ConcluirAsync(
            new ConcluirProcedimentoCommand(
                fichaId,
                profissional.Id,
                request.ValorTotal!.Value,
                request.ValorSinal!.Value,
                request.FormaPagamento!.Value,
                request.AssinaturaDesenhada,
                request.Tatuagem is null
                    ? null
                    : new RegistroTatuagemCommand(
                        request.Tatuagem.ArteEfetivamenteTatuada,
                        request.Tatuagem.MaterialUtilizado,
                        request.Tatuagem.LocalTatuagem,
                        request.Tatuagem.Observacoes),
                request.Piercing is null
                    ? null
                    : new RegistroPiercingCommand(
                        request.Piercing.JoiaUtilizada,
                        request.Piercing.AgulhaUtilizada,
                        request.Piercing.LocalPerfuracao,
                        request.Piercing.Observacoes)),
            cancellationToken);

        if (resultado.Resultado == StatusConclusaoProcedimento.Concluido)
        {
            return Ok(new OperacaoFichaResponse(
                resultado.FichaId!.Value,
                resultado.StatusFicha!.Value.ToString()));
        }

        return CriarProblemaOperacao(
            resultado.Resultado == StatusConclusaoProcedimento.FichaNaoEncontrada,
            resultado.Resultado ==
                StatusConclusaoProcedimento.ProfissionalNaoResponsavel,
            "Confira o registro técnico e o estado atual da ficha.");
    }

    private ObjectResult CriarProblemaOperacao(
        bool fichaNaoEncontrada,
        bool profissionalNaoResponsavel,
        string detalhe)
    {
        if (fichaNaoEncontrada)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Ficha não encontrada.");
        }

        if (profissionalNaoResponsavel)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Operação não permitida.",
                detail: "Somente o profissional responsável pode alterar esta ficha.");
        }

        return Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Operação não concluída.",
            detail: detalhe);
    }
}

public sealed record OperacaoFichaResponse(Guid FichaId, string Status);

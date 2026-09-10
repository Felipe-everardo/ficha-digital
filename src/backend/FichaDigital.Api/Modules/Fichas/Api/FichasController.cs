using FichaDigital.Api.Modules.Fichas.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FichaDigital.Api.Modules.Fichas.Api;

[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/fichas")]
public sealed class FichasController(ConsultaFichas consultaFichas)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<FichasPaginadasResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FichasPaginadasResponse>> Listar(
        [FromQuery] ListarFichasRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await consultaFichas.ListarAsync(
            new FiltroConsultaFichas(
                request.Busca,
                request.ProfissionalId,
                request.TipoProcedimento,
                request.Status,
                request.CriadaDe,
                request.CriadaAte,
                request.ConcluidaDe,
                request.ConcluidaAte,
                request.Pagina,
                request.TamanhoPagina),
            cancellationToken);

        return Ok(new FichasPaginadasResponse(
            resultado.Itens.Select(CriarResumo).ToList(),
            resultado.Pagina,
            resultado.TamanhoPagina,
            resultado.TotalItens,
            resultado.TotalPaginas));
    }

    [HttpGet("{fichaId:guid}")]
    [ProducesResponseType<FichaDetalheResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FichaDetalheResponse>> ObterDetalhe(
        Guid fichaId,
        CancellationToken cancellationToken)
    {
        var ficha = await consultaFichas.ObterDetalheAsync(
            fichaId,
            cancellationToken);

        if (ficha is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Ficha não encontrada.",
                detail: "Não existe uma ficha com o identificador informado.");
        }

        return Ok(new FichaDetalheResponse(
            ficha.Id,
            ficha.Status,
            ficha.CriadaEmUtc,
            ficha.ConviteExpiraEmUtc,
            ficha.ConviteExpirado,
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.TipoProcedimento,
            new ClienteFichaDetalheResponse(
                ficha.Cliente.Id,
                ficha.Cliente.NomeReferencia,
                ficha.Cliente.NomeCompleto,
                ficha.Cliente.NomeSocial,
                ficha.Cliente.NomeParaExibicao,
                ficha.Cliente.Pronomes,
                ficha.Cliente.DataNascimento,
                ficha.Cliente.Celular,
                ficha.Cliente.Email,
                ficha.Cliente.Instagram,
                ficha.Cliente.ContatoEmergenciaNome,
                ficha.Cliente.ContatoEmergenciaCelular,
                ficha.Cliente.DadosPessoaisPreenchidosEmUtc),
            ficha.QuestionarioSaude is null
                ? null
                : new QuestionarioSaudeDetalheResponse(
                    ficha.QuestionarioSaude.Versao,
                    ficha.QuestionarioSaude.TemDiabetes,
                    ficha.QuestionarioSaude.TipoDiabetes,
                    ficha.QuestionarioSaude.PossuiPressaoAlta,
                    ficha.QuestionarioSaude.TemAlergia,
                    ficha.QuestionarioSaude.DescricaoAlergia,
                    ficha.QuestionarioSaude.PossuiCondicaoCardiaca,
                    ficha.QuestionarioSaude.TemEpilepsia,
                    ficha.QuestionarioSaude.TemHemofilia,
                    ficha.QuestionarioSaude.UsaMarcaPasso,
                    ficha.QuestionarioSaude.EstaGravidaOuAmamentando,
                    ficha.QuestionarioSaude.RespondidoEmUtc),
            ficha.AceiteTermo is null
                ? null
                : new AceiteTermoResumoResponse(
                    ficha.AceiteTermo.VersaoTermo,
                    ficha.AceiteTermo.NomeAssinante,
                    ficha.AceiteTermo.AceitoEmUtc,
                    ficha.AceiteTermo.ConfirmouMaioridade,
                    ficha.AceiteTermo.ConfirmouDadosPessoais,
                    ficha.AceiteTermo.ConfirmouQuestionarioSaude,
                    ficha.AceiteTermo.EvidenciaHash,
                    ficha.AceiteTermo.EvidenciaIntegra)));
    }

    private static FichaResumoResponse CriarResumo(FichaConsultada ficha)
    {
        return new FichaResumoResponse(
            ficha.Id,
            ficha.ClienteId,
            ficha.ClienteNome,
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.TipoProcedimento,
            ficha.Status,
            ficha.CriadaEmUtc,
            ficha.ConcluidaEmUtc,
            ficha.ConviteExpiraEmUtc,
            ficha.ConviteExpirado);
    }
}

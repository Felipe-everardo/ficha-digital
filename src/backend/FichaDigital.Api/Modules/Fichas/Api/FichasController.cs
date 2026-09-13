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
            ficha.VersaoModelo,
            ficha.VersaoQuestionario,
            ficha.VersaoTermo,
            ficha.CnpjApresentado,
            new ClienteFichaDetalheResponse(
                ficha.Cliente.Id,
                ficha.Cliente.NomeReferencia,
                ficha.Cliente.NomeCompleto,
                ficha.Cliente.NomeSocial,
                ficha.Cliente.NomeParaExibicao,
                ficha.Cliente.Pronomes,
                ficha.Cliente.EstadoCivil,
                ficha.Cliente.DataNascimento,
                ficha.Cliente.Cpf,
                ficha.Cliente.Celular,
                ficha.Cliente.TelefoneAdicional,
                ficha.Cliente.Email,
                ficha.Cliente.Instagram,
                ficha.Cliente.ContatoEmergenciaNome,
                ficha.Cliente.ContatoEmergenciaCelular,
                ficha.Cliente.Cep,
                ficha.Cliente.Logradouro,
                ficha.Cliente.Numero,
                ficha.Cliente.Complemento,
                ficha.Cliente.Bairro,
                ficha.Cliente.Cidade,
                ficha.Cliente.Estado,
                ficha.Cliente.DadosPessoaisPreenchidosEmUtc),
            ficha.QuestionarioSaude is null
                ? null
                : new QuestionarioSaudeDetalheResponse(
                    ficha.QuestionarioSaude.Versao,
                    ficha.QuestionarioSaude.TemDiabetes,
                    ficha.QuestionarioSaude.TipoDiabetes,
                    ficha.QuestionarioSaude.TeveAnemia,
                    ficha.QuestionarioSaude.DescricaoAnemia,
                    ficha.QuestionarioSaude.TeveHepatite,
                    ficha.QuestionarioSaude.TipoHepatite,
                    ficha.QuestionarioSaude.PossuiPressaoAlta,
                    ficha.QuestionarioSaude.TemAlergia,
                    ficha.QuestionarioSaude.DescricaoAlergia,
                    ficha.QuestionarioSaude.PossuiCondicaoCardiaca,
                    ficha.QuestionarioSaude.TemEpilepsia,
                    ficha.QuestionarioSaude.TemHemofilia,
                    ficha.QuestionarioSaude.PossuiDoencaTransmissivel,
                    ficha.QuestionarioSaude.DescricaoDoencaTransmissivel,
                    ficha.QuestionarioSaude.UsaMarcaPasso,
                    ficha.QuestionarioSaude.Fuma,
                    ficha.QuestionarioSaude.ConsumiuBebidaAlcoolicaUltimas24Horas,
                    ficha.QuestionarioSaude.UsaMedicacao,
                    ficha.QuestionarioSaude.DescricaoMedicacao,
                    ficha.QuestionarioSaude.EstaGravidaOuAmamentando,
                    ficha.QuestionarioSaude.RespondidoEmUtc),
            ficha.AceiteTermo is null
                ? null
                : new AceiteTermoResumoResponse(
                    ficha.AceiteTermo.VersaoTermo,
                    ficha.AceiteTermo.NomeAssinante,
                    ficha.AceiteTermo.AceitoEmUtc,
                    ficha.AceiteTermo.ConfirmouLeituraEAutorizacao,
                    ficha.AceiteTermo.AssinaturaDesenhada,
                    ficha.AceiteTermo.EvidenciaHash,
                    ficha.AceiteTermo.EvidenciaIntegra),
            ficha.RevisaoProfissional is null
                ? null
                : new RevisaoProfissionalResponse(
                    ficha.RevisaoProfissional.ProfissionalNome,
                    ficha.RevisaoProfissional.DadosDaFichaConferidos,
                    ficha.RevisaoProfissional.RevisadaEmUtc),
            ficha.RegistroProcedimento is null
                ? null
                : new RegistroProcedimentoResponse(
                    ficha.RegistroProcedimento.TipoProcedimento,
                    ficha.RegistroProcedimento.ProfissionalNome,
                    ficha.RegistroProcedimento.ArteEfetivamenteTatuada,
                    ficha.RegistroProcedimento.MaterialUtilizado,
                    ficha.RegistroProcedimento.LocalTatuagem,
                    ficha.RegistroProcedimento.JoiaUtilizada,
                    ficha.RegistroProcedimento.AgulhaUtilizada,
                    ficha.RegistroProcedimento.LocalPerfuracao,
                    ficha.RegistroProcedimento.Observacoes,
                    ficha.RegistroProcedimento.ValorTotal,
                    ficha.RegistroProcedimento.ValorSinal,
                    ficha.RegistroProcedimento.ValorTotal -
                        ficha.RegistroProcedimento.ValorSinal,
                    ficha.RegistroProcedimento.FormaPagamento,
                    ficha.RegistroProcedimento.NomeProfissionalAssinante,
                    ficha.RegistroProcedimento.AssinaturaDesenhada,
                    ficha.RegistroProcedimento.RegistradoEmUtc,
                    ficha.RegistroProcedimento.EvidenciaHash,
                    ficha.RegistroProcedimento.EvidenciaIntegra)));
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

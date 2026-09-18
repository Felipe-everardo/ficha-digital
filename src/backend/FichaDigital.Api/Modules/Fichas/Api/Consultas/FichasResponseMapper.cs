using FichaDigital.Api.Modules.Fichas.Application;

namespace FichaDigital.Api.Modules.Fichas.Api;

internal static class FichasResponseMapper
{
    public static FichasPaginadasResponse ToResponse(
        this PaginaFichasConsultada pagina)
    {
        return new FichasPaginadasResponse(
            pagina.Itens.Select(ToResumoResponse).ToList(),
            pagina.Pagina,
            pagina.TamanhoPagina,
            pagina.TotalItens,
            pagina.TotalPaginas);
    }

    public static FichaDetalheResponse ToResponse(
        this DetalheFichaConsultada ficha)
    {
        return new FichaDetalheResponse(
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
            ToClienteResponse(ficha.Cliente),
            ficha.QuestionarioSaude is null
                ? null
                : ToQuestionarioResponse(ficha.QuestionarioSaude),
            ficha.AceiteTermo is null
                ? null
                : ToAceiteResponse(ficha.AceiteTermo),
            ficha.RevisaoProfissional is null
                ? null
                : ToRevisaoResponse(ficha.RevisaoProfissional),
            ficha.RegistroProcedimento is null
                ? null
                : ToRegistroResponse(ficha.RegistroProcedimento));
    }

    private static FichaResumoResponse ToResumoResponse(FichaConsultada ficha)
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

    private static ClienteFichaDetalheResponse ToClienteResponse(
        ClienteDaFichaConsultado cliente)
    {
        return new ClienteFichaDetalheResponse(
            cliente.Id,
            cliente.NomeReferencia,
            cliente.NomeCompleto,
            cliente.NomeSocial,
            cliente.NomeParaExibicao,
            cliente.Pronomes,
            cliente.EstadoCivil,
            cliente.DataNascimento,
            cliente.Cpf,
            cliente.Celular,
            cliente.TelefoneAdicional,
            cliente.Email,
            cliente.Instagram,
            cliente.ContatoEmergenciaNome,
            cliente.ContatoEmergenciaCelular,
            cliente.Cep,
            cliente.Logradouro,
            cliente.Numero,
            cliente.Complemento,
            cliente.Bairro,
            cliente.Cidade,
            cliente.Estado,
            cliente.DadosPessoaisPreenchidosEmUtc);
    }

    private static QuestionarioSaudeDetalheResponse ToQuestionarioResponse(
        QuestionarioSaudeConsultado questionario)
    {
        return new QuestionarioSaudeDetalheResponse(
            questionario.Versao,
            questionario.TemDiabetes,
            questionario.TipoDiabetes,
            questionario.TeveAnemia,
            questionario.DescricaoAnemia,
            questionario.TeveHepatite,
            questionario.TipoHepatite,
            questionario.PossuiPressaoAlta,
            questionario.TemAlergia,
            questionario.DescricaoAlergia,
            questionario.PossuiCondicaoCardiaca,
            questionario.TemEpilepsia,
            questionario.TemHemofilia,
            questionario.PossuiDoencaTransmissivel,
            questionario.DescricaoDoencaTransmissivel,
            questionario.UsaMarcaPasso,
            questionario.Fuma,
            questionario.ConsumiuBebidaAlcoolicaUltimas24Horas,
            questionario.UsaMedicacao,
            questionario.DescricaoMedicacao,
            questionario.EstaGravidaOuAmamentando,
            questionario.RespondidoEmUtc);
    }

    private static AceiteTermoResumoResponse ToAceiteResponse(
        AceiteTermoConsultado aceite)
    {
        return new AceiteTermoResumoResponse(
            aceite.VersaoTermo,
            aceite.NomeAssinante,
            aceite.AceitoEmUtc,
            aceite.ConfirmouLeituraEAutorizacao,
            aceite.AssinaturaDesenhada,
            aceite.EvidenciaHash,
            aceite.EvidenciaIntegra);
    }

    private static RevisaoProfissionalResponse ToRevisaoResponse(
        RevisaoProfissionalConsultada revisao)
    {
        return new RevisaoProfissionalResponse(
            revisao.ProfissionalNome,
            revisao.DadosDaFichaConferidos,
            revisao.RevisadaEmUtc);
    }

    private static RegistroProcedimentoResponse ToRegistroResponse(
        RegistroProcedimentoConsultado registro)
    {
        return new RegistroProcedimentoResponse(
            registro.TipoProcedimento,
            registro.ProfissionalNome,
            registro.ArteEfetivamenteTatuada,
            registro.MaterialUtilizado,
            registro.LocalTatuagem,
            registro.JoiaUtilizada,
            registro.AgulhaUtilizada,
            registro.LocalPerfuracao,
            registro.Observacoes,
            registro.ValorTotal,
            registro.ValorSinal,
            registro.ValorTotal - registro.ValorSinal,
            registro.FormaPagamento,
            registro.NomeProfissionalAssinante,
            registro.AssinaturaDesenhada,
            registro.RegistradoEmUtc,
            registro.EvidenciaHash,
            registro.EvidenciaIntegra);
    }
}

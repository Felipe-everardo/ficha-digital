using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class ObterFichaDetalheService(
    FichaDigitalDbContext dbContext,
    TimeProvider timeProvider,
    CalculadorHashConteudo calculadorHash)
{
    public async Task<DetalheFichaConsultada?> ObterAsync(
        Guid fichaId,
        CancellationToken cancellationToken)
    {
        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == fichaId, cancellationToken);

        if (ficha is null)
        {
            return null;
        }

        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleAsync(item => item.Id == ficha.ClienteId, cancellationToken);
        var dadosDaFicha = await dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var questionario = await dbContext.QuestionariosSaude
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var aceite = await dbContext.AceitesTermoConsentimento
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var revisao = await dbContext.RevisoesProfissionais
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var registroTatuagem = await dbContext.RegistrosTatuagem
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var registroPiercing = await dbContext.RegistrosPiercing
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);
        var conviteExpiraEmUtc = await dbContext.ConvitesFicha
            .AsNoTracking()
            .Where(convite => convite.FichaId == ficha.Id)
            .Select(convite => (DateTimeOffset?)convite.ExpiraEmUtc)
            .SingleOrDefaultAsync(cancellationToken);
        var instanteAtual = timeProvider.GetUtcNow();

        return new DetalheFichaConsultada(
            ficha.Id,
            ficha.Status.ToString(),
            ficha.CriadaEmUtc,
            conviteExpiraEmUtc,
            conviteExpiraEmUtc is not null &&
                conviteExpiraEmUtc <= instanteAtual,
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.TipoProcedimento.ToString(),
            ficha.VersaoModelo,
            ficha.VersaoQuestionario,
            ficha.VersaoTermo,
            ficha.CnpjApresentado,
            new ClienteDaFichaConsultado(
                cliente.Id,
                cliente.NomeReferencia,
                dadosDaFicha?.NomeCompleto,
                dadosDaFicha?.NomeSocial,
                dadosDaFicha?.NomeParaExibicao ?? cliente.NomeReferencia,
                dadosDaFicha?.Pronomes,
                dadosDaFicha?.EstadoCivil,
                dadosDaFicha?.DataNascimento,
                dadosDaFicha?.Cpf,
                dadosDaFicha?.Celular,
                dadosDaFicha?.TelefoneAdicional,
                dadosDaFicha?.Email,
                dadosDaFicha?.Instagram,
                dadosDaFicha?.ContatoEmergenciaNome,
                dadosDaFicha?.ContatoEmergenciaCelular,
                dadosDaFicha?.Cep,
                dadosDaFicha?.Logradouro,
                dadosDaFicha?.Numero,
                dadosDaFicha?.Complemento,
                dadosDaFicha?.Bairro,
                dadosDaFicha?.Cidade,
                dadosDaFicha?.Estado,
                dadosDaFicha?.ConfirmadosEmUtc),
            questionario is null
                ? null
                : new QuestionarioSaudeConsultado(
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
                    questionario.RespondidoEmUtc),
            aceite is null
                ? null
                : new AceiteTermoConsultado(
                    aceite.VersaoTermo,
                    aceite.NomeAssinante,
                    aceite.AceitoEmUtc,
                    aceite.ConfirmouLeituraEAutorizacao,
                    aceite.AssinaturaDesenhada,
                    aceite.EvidenciaHash,
                    string.Equals(
                        calculadorHash.Calcular(aceite.EvidenciaJson),
                        aceite.EvidenciaHash,
                        StringComparison.OrdinalIgnoreCase)),
            revisao is null
                ? null
                : new RevisaoProfissionalConsultada(
                    revisao.ProfissionalNome,
                    revisao.DadosDaFichaConferidos,
                    revisao.RevisadaEmUtc),
            CriarRegistroConsultado(registroTatuagem, registroPiercing));
    }

    private RegistroProcedimentoConsultado? CriarRegistroConsultado(
        RegistroTatuagem? tatuagem,
        RegistroPiercing? piercing)
    {
        if (tatuagem is not null)
        {
            return new RegistroProcedimentoConsultado(
                TipoProcedimento.Tatuagem.ToString(),
                tatuagem.ProfissionalNome,
                tatuagem.ArteEfetivamenteTatuada,
                tatuagem.MaterialUtilizado,
                tatuagem.LocalTatuagem,
                null,
                null,
                null,
                tatuagem.Observacoes,
                tatuagem.ValorTotal,
                tatuagem.ValorSinal,
                tatuagem.FormaPagamento.ToString(),
                tatuagem.NomeProfissionalAssinante,
                tatuagem.AssinaturaDesenhada,
                tatuagem.RegistradoEmUtc,
                tatuagem.EvidenciaHash,
                string.Equals(
                    calculadorHash.Calcular(tatuagem.EvidenciaJson),
                    tatuagem.EvidenciaHash,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (piercing is not null)
        {
            return new RegistroProcedimentoConsultado(
                TipoProcedimento.Piercing.ToString(),
                piercing.ProfissionalNome,
                null,
                null,
                null,
                piercing.JoiaUtilizada,
                piercing.AgulhaUtilizada,
                piercing.LocalPerfuracao,
                piercing.Observacoes,
                piercing.ValorTotal,
                piercing.ValorSinal,
                piercing.FormaPagamento.ToString(),
                piercing.NomeProfissionalAssinante,
                piercing.AssinaturaDesenhada,
                piercing.RegistradoEmUtc,
                piercing.EvidenciaHash,
                string.Equals(
                    calculadorHash.Calcular(piercing.EvidenciaJson),
                    piercing.EvidenciaHash,
                    StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }
}

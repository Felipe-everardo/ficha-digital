using System.Text.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class AceitarTermoConsentimentoService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    CalculadorHashConteudo calculadorHash,
    TimeProvider timeProvider)
{
    private const int VersaoEvidenciaAtual = 1;

    private static readonly JsonSerializerOptions OpcoesJsonEvidencia = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<ResultadoAceiteTermoConsentimento> AceitarAsync(
        AceitarTermoConsentimentoCommand command,
        CancellationToken cancellationToken)
    {
        var tokenHash = geradorToken.CalcularHash(command.TokenOriginal);
        var convite = await dbContext.ConvitesFicha
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TokenHash == tokenHash,
                cancellationToken);

        if (convite is null)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConviteNaoEncontrado);
        }

        if (convite.EstaExpirado(timeProvider.GetUtcNow()))
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConviteExpirado);
        }

        var ficha = await dbContext.Fichas
            .SingleOrDefaultAsync(
                item => item.Id == convite.FichaId,
                cancellationToken);

        if (ficha is null)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConviteNaoEncontrado);
        }

        if (ficha.Status != StatusFicha.EmPreenchimento)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.FichaIndisponivel);
        }

        var questionario = await dbContext.QuestionariosSaude
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (questionario is null)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.QuestionarioPendente);
        }

        var dadosDaFicha = await dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (dadosDaFicha is null)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.DadosPessoaisPendentes);
        }

        if (!RegraMaioridade.EhMaiorDeIdade(
                dadosDaFicha.DataNascimento,
                timeProvider.GetUtcNow()))
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ClienteMenorDeIdade);
        }

        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == ficha.ClienteId,
                cancellationToken);

        if (cliente is null)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConviteNaoEncontrado);
        }

        var aceiteJaExiste = await dbContext.AceitesTermoConsentimento
            .AnyAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (aceiteJaExiste)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.JaAceito);
        }

        var conteudoHashAtual = calculadorHash.Calcular(
            TermoConsentimentoAtual.Conteudo);

        if (command.VersaoTermo != TermoConsentimentoAtual.Versao ||
            !string.Equals(
                command.ConteudoHash,
                conteudoHashAtual,
                StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.TermoDesatualizado);
        }

        var aceitoEmUtc = timeProvider.GetUtcNow();
        var evidenciaJson = CriarEvidenciaJson(
            convite,
            ficha,
            cliente,
            dadosDaFicha,
            questionario,
            command,
            conteudoHashAtual,
            aceitoEmUtc);
        var evidenciaHash = calculadorHash.Calcular(evidenciaJson);
        var aceite = new AceiteTermoConsentimento(
            ficha.Id,
            convite.Id,
            TermoConsentimentoAtual.Versao,
            TermoConsentimentoAtual.Conteudo,
            conteudoHashAtual,
            command.NomeAssinante,
            command.ConfirmouMaioridade,
            command.ConfirmouDadosPessoais,
            command.ConfirmouQuestionarioSaude,
            VersaoEvidenciaAtual,
            evidenciaJson,
            evidenciaHash,
            command.EnderecoIp,
            command.AgenteUsuario,
            aceitoEmUtc);

        ficha.Concluir();
        dbContext.AceitesTermoConsentimento.Add(aceite);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoAceiteTermoConsentimento(
            StatusAceiteTermoConsentimento.Aceito,
            aceite.Id,
            aceite.FichaId,
            aceite.VersaoTermo,
            aceite.AceitoEmUtc,
            aceite.EvidenciaHash);
    }

    private static string CriarEvidenciaJson(
        ConviteFicha convite,
        Ficha ficha,
        Cliente cliente,
        DadosPessoaisFicha dadosDaFicha,
        QuestionarioSaude questionario,
        AceitarTermoConsentimentoCommand command,
        string conteudoHashTermo,
        DateTimeOffset aceitoEmUtc)
    {
        var evidencia = new
        {
            versaoEvidencia = VersaoEvidenciaAtual,
            aceite = new
            {
                fichaId = ficha.Id,
                conviteId = convite.Id,
                convite.CriadoEmUtc,
                convite.ExpiraEmUtc,
                command.NomeAssinante,
                command.ConfirmouMaioridade,
                command.ConfirmouDadosPessoais,
                command.ConfirmouQuestionarioSaude,
                aceitoEmUtc
            },
            procedimento = new
            {
                ficha.ProfissionalResponsavelId,
                ficha.ProfissionalResponsavelNome,
                tipoProcedimento = ficha.TipoProcedimento.ToString(),
                ficha.CriadaEmUtc
            },
            cliente = new
            {
                cliente.Id,
                cliente.NomeReferencia,
                dadosDaFicha.NomeCompleto,
                dadosDaFicha.NomeSocial,
                dadosDaFicha.Pronomes,
                dadosDaFicha.DataNascimento,
                dadosDaFicha.Celular,
                dadosDaFicha.Email,
                dadosDaFicha.Instagram,
                dadosDaFicha.ContatoEmergenciaNome,
                dadosDaFicha.ContatoEmergenciaCelular,
                dadosDaFicha.ConfirmadosEmUtc
            },
            questionarioSaude = new
            {
                questionario.Versao,
                questionario.TemDiabetes,
                questionario.TipoDiabetes,
                questionario.PossuiPressaoAlta,
                questionario.TemAlergia,
                questionario.DescricaoAlergia,
                questionario.PossuiCondicaoCardiaca,
                questionario.TemEpilepsia,
                questionario.TemHemofilia,
                questionario.UsaMarcaPasso,
                questionario.EstaGravidaOuAmamentando,
                questionario.RespondidoEmUtc
            },
            dadosTecnicos = new
            {
                command.EnderecoIp,
                command.AgenteUsuario
            },
            termo = new
            {
                versao = TermoConsentimentoAtual.Versao,
                conteudoHash = conteudoHashTermo,
                conteudo = TermoConsentimentoAtual.Conteudo
            }
        };

        return JsonSerializer.Serialize(evidencia, OpcoesJsonEvidencia);
    }
}

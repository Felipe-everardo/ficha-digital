using System.Text.Json;
using FichaDigital.Api.Infrastructure.Auditing;
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
    ResolvedorModeloFicha resolvedorModelo,
    TimeProvider timeProvider,
    AuditoriaService auditoriaService)
{
    private const int VersaoEvidenciaAtual = 4;

    private static readonly JsonSerializerOptions OpcoesJsonEvidencia = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<ResultadoAceiteTermoConsentimento> AceitarAsync(
        AceitarTermoConsentimentoCommand command,
        string correlacaoId,
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

        var fluxoVersionado = ficha.VersaoModelo is not null;
        var statusValido = fluxoVersionado
            ? ficha.Status is StatusFicha.AnamnesePreenchida or
                StatusFicha.AguardandoConsentimento
            : ficha.Status == StatusFicha.EmPreenchimento;

        if (!statusValido)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.FichaIndisponivel);
        }

        if (!command.ConfirmouLeituraEAutorizacao)
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ConfirmacaoObrigatoria);
        }

        if (!RegraMaioridade.EhMaiorDeIdade(
                dadosDaFicha.DataNascimento,
                timeProvider.GetUtcNow()))
        {
            return new ResultadoAceiteTermoConsentimento(
                StatusAceiteTermoConsentimento.ClienteMenorDeIdade);
        }

        string? assinaturaDesenhada = null;

        if (fluxoVersionado)
        {
            if (!ComparadorNomes.Correspondem(
                    command.NomeAssinante,
                    dadosDaFicha.NomeCompleto))
            {
                return new ResultadoAceiteTermoConsentimento(
                    StatusAceiteTermoConsentimento.NomeAssinanteDivergente);
            }

            try
            {
                assinaturaDesenhada = AssinaturaDesenhada
                    .ValidarENormalizar(
                        command.AssinaturaDesenhada,
                        nameof(command.AssinaturaDesenhada));
            }
            catch (ArgumentException)
            {
                return new ResultadoAceiteTermoConsentimento(
                    StatusAceiteTermoConsentimento.AssinaturaInvalida);
            }

        }

        var modelo = resolvedorModelo.ObterModeloDaFicha(ficha);
        var conteudoHashAtual = calculadorHash.Calcular(
            modelo.ConteudoTermo);

        if (command.VersaoTermo != modelo.VersaoTermo ||
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
            modelo,
            command,
            assinaturaDesenhada,
            conteudoHashAtual,
            aceitoEmUtc);
        var evidenciaHash = calculadorHash.Calcular(evidenciaJson);
        var aceite = new AceiteTermoConsentimento(
            ficha.Id,
            convite.Id,
            modelo.VersaoTermo,
            modelo.ConteudoTermo,
            conteudoHashAtual,
            command.NomeAssinante,
            command.ConfirmouLeituraEAutorizacao,
            VersaoEvidenciaAtual,
            evidenciaJson,
            evidenciaHash,
            command.EnderecoIp,
            command.AgenteUsuario,
            aceitoEmUtc,
            assinaturaDesenhada);

        if (fluxoVersionado)
        {
            ficha.AutorizarProcedimento();
        }
        else
        {
            ficha.ConcluirFluxoLegado();
        }

        dbContext.AceitesTermoConsentimento.Add(aceite);
        auditoriaService.AdicionarAcaoDoCliente(
            "Consentimento assinado e procedimento autorizado",
            ficha.Id,
            correlacaoId);
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
        ModeloFichaSelecionado modelo,
        AceitarTermoConsentimentoCommand command,
        string? assinaturaDesenhada,
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
                command.ConfirmouLeituraEAutorizacao,
                assinaturaDesenhada,
                aceitoEmUtc
            },
            procedimento = new
            {
                ficha.ProfissionalResponsavelId,
                ficha.ProfissionalResponsavelNome,
                tipoProcedimento = ficha.TipoProcedimento.ToString(),
                modelo.VersaoModelo,
                modelo.VersaoQuestionario,
                modelo.VersaoTermo,
                modelo.CnpjApresentado,
                ficha.CriadaEmUtc
            },
            cliente = new
            {
                cliente.Id,
                cliente.NomeReferencia,
                dadosDaFicha.NomeCompleto,
                dadosDaFicha.NomeSocial,
                dadosDaFicha.Pronomes,
                dadosDaFicha.EstadoCivil,
                dadosDaFicha.DataNascimento,
                dadosDaFicha.Cpf,
                dadosDaFicha.Celular,
                dadosDaFicha.TelefoneAdicional,
                dadosDaFicha.Email,
                dadosDaFicha.Instagram,
                dadosDaFicha.ContatoEmergenciaNome,
                dadosDaFicha.ContatoEmergenciaCelular,
                dadosDaFicha.Cep,
                dadosDaFicha.Logradouro,
                dadosDaFicha.Numero,
                dadosDaFicha.Complemento,
                dadosDaFicha.Bairro,
                dadosDaFicha.Cidade,
                dadosDaFicha.Estado,
                dadosDaFicha.ConfirmadosEmUtc
            },
            questionarioSaude = new
            {
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
                questionario.RespondidoEmUtc
            },
            dadosTecnicos = new
            {
                command.EnderecoIp,
                command.AgenteUsuario
            },
            termo = new
            {
                versao = modelo.VersaoTermo,
                conteudoHash = conteudoHashTermo,
                conteudo = modelo.ConteudoTermo
            }
        };

        return JsonSerializer.Serialize(evidencia, OpcoesJsonEvidencia);
    }

}

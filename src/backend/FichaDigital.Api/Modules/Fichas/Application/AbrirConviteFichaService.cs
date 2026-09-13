using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class AbrirConviteFichaService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    ResolvedorModeloFicha resolvedorModelo,
    TimeProvider timeProvider)
{
    public async Task<ResultadoAberturaConvite> AbrirAsync(
        string tokenOriginal,
        CancellationToken cancellationToken)
    {
        var tokenHash = geradorToken.CalcularHash(tokenOriginal);

        var convite = await dbContext.ConvitesFicha
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TokenHash == tokenHash,
                cancellationToken);

        if (convite is null)
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.NaoEncontrado);
        }

        if (convite.EstaExpirado(timeProvider.GetUtcNow()))
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.Expirado);
        }

        var ficha = await dbContext.Fichas
            .SingleOrDefaultAsync(
                item => item.Id == convite.FichaId,
                cancellationToken);

        if (ficha is null)
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.NaoEncontrado);
        }

        if (ficha.Status == StatusFicha.ConviteEnviado)
        {
            ficha.IniciarPreenchimento();
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (ficha.Status is not (
                     StatusFicha.EmPreenchimento or
                     StatusFicha.AnamnesePreenchida or
                     StatusFicha.AguardandoConsentimento))
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.Indisponivel);
        }

        var questionario = await dbContext.QuestionariosSaude
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        var dadosDaFicha = await dbContext.DadosPessoaisFichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == ficha.ClienteId,
                cancellationToken);

        if (cliente is null)
        {
            return new ResultadoAberturaConvite(
                StatusAberturaConvite.NaoEncontrado);
        }

        var modelo = resolvedorModelo.ObterModeloDaFicha(ficha);

        return new ResultadoAberturaConvite(
            StatusAberturaConvite.Aberto,
            ficha.Id,
            ficha.Status,
            questionario is not null,
            dadosDaFicha is not null,
            cliente.NomeReferencia,
            ficha.ProfissionalResponsavelNome,
            ficha.TipoProcedimento,
            dadosDaFicha is null
                ? new DadosPessoaisConvite(
                    cliente.NomeCompleto,
                    cliente.NomeSocial,
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
                    cliente.Estado)
                : new DadosPessoaisConvite(
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
                    dadosDaFicha.Estado),
            questionario is null
                ? null
                : new QuestionarioSaudeConvite(
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
            new TermoConsentimentoConvite(
                modelo.VersaoTermo,
                modelo.ConteudoTermo));
    }
}

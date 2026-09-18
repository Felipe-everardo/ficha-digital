using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Infrastructure.Auditing;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using FichaDigital.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class PreencherDadosPessoaisService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    TimeProvider timeProvider,
    AuditoriaService auditoriaService)
{
    public async Task<ResultadoPreenchimentoDadosPessoais> PreencherAsync(
        PreencherDadosPessoaisCommand command,
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
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.ConviteNaoEncontrado);
        }

        if (convite.EstaExpirado(timeProvider.GetUtcNow()))
        {
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.ConviteExpirado);
        }

        var ficha = await dbContext.Fichas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == convite.FichaId,
                cancellationToken);

        if (ficha is null)
        {
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.ConviteNaoEncontrado);
        }

        if (ficha.Status != StatusFicha.EmPreenchimento)
        {
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.FichaIndisponivel);
        }

        var cliente = await dbContext.Clientes
            .SingleOrDefaultAsync(
                item => item.Id == ficha.ClienteId,
                cancellationToken);

        if (cliente is null)
        {
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.ClienteNaoEncontrado);
        }

        var preenchidosEmUtc = timeProvider.GetUtcNow();

        var dadosInformados = new DadosPessoaisInformados(
            command.NomeCompleto,
            command.NomeSocial,
            command.Pronomes,
            command.EstadoCivil,
            command.DataNascimento,
            command.Cpf,
            command.Celular,
            command.TelefoneAdicional,
            command.Email,
            command.Instagram,
            command.ContatoEmergenciaNome,
            command.ContatoEmergenciaCelular,
            command.Cep,
            command.Logradouro,
            command.Numero,
            command.Complemento,
            command.Bairro,
            command.Cidade,
            command.Estado);

        if (!RegraMaioridade.EhMaiorDeIdade(
                dadosInformados.DataNascimento,
                preenchidosEmUtc))
        {
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.ClienteMenorDeIdade);
        }

        if (cliente.DadosPessoaisPreenchidos)
        {
            cliente.AtualizarDadosPessoais(
                dadosInformados,
                preenchidosEmUtc);
        }
        else
        {
            cliente.PreencherDadosPessoais(
                dadosInformados,
                preenchidosEmUtc);
        }

        var dadosDaFicha = await dbContext.DadosPessoaisFichas
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (dadosDaFicha is null)
        {
            dadosDaFicha = new DadosPessoaisFicha(
                ficha.Id,
                dadosInformados,
                preenchidosEmUtc);
            dbContext.DadosPessoaisFichas.Add(dadosDaFicha);
        }
        else
        {
            dadosDaFicha.Atualizar(
                dadosInformados,
                preenchidosEmUtc);
        }

        auditoriaService.AdicionarAcaoDoCliente(
            "Dados pessoais confirmados",
            ficha.Id,
            correlacaoId);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoPreenchimentoDadosPessoais(
            StatusPreenchimentoDadosPessoais.Preenchidos,
            ficha.Id,
            cliente.Id,
            dadosDaFicha.NomeParaExibicao,
            preenchidosEmUtc);
    }
}

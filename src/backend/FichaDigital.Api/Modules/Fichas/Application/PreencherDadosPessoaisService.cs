using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class PreencherDadosPessoaisService(
    FichaDigitalDbContext dbContext,
    GeradorTokenConvite geradorToken,
    TimeProvider timeProvider)
{
    public async Task<ResultadoPreenchimentoDadosPessoais> PreencherAsync(
        PreencherDadosPessoaisCommand command,
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

        if (!RegraMaioridade.EhMaiorDeIdade(
                command.DataNascimento,
                preenchidosEmUtc))
        {
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.ClienteMenorDeIdade);
        }

        if (cliente.DadosPessoaisPreenchidos)
        {
            cliente.AtualizarDadosPessoais(
                command.NomeCompleto,
                command.NomeSocial,
                command.Pronomes,
                command.DataNascimento,
                command.Celular,
                command.Email,
                command.Instagram,
                command.ContatoEmergenciaNome,
                command.ContatoEmergenciaCelular,
                preenchidosEmUtc);
        }
        else
        {
            cliente.PreencherDadosPessoais(
                command.NomeCompleto,
                command.NomeSocial,
                command.Pronomes,
                command.DataNascimento,
                command.Celular,
                command.Email,
                command.Instagram,
                command.ContatoEmergenciaNome,
                command.ContatoEmergenciaCelular,
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
                cliente.NomeCompleto!,
                cliente.NomeSocial,
                cliente.Pronomes,
                cliente.DataNascimento!.Value,
                cliente.Celular!,
                cliente.Email,
                cliente.Instagram,
                cliente.ContatoEmergenciaNome,
                cliente.ContatoEmergenciaCelular,
                preenchidosEmUtc);
            dbContext.DadosPessoaisFichas.Add(dadosDaFicha);
        }
        else
        {
            dadosDaFicha.Atualizar(
                cliente.NomeCompleto!,
                cliente.NomeSocial,
                cliente.Pronomes,
                cliente.DataNascimento!.Value,
                cliente.Celular!,
                cliente.Email,
                cliente.Instagram,
                cliente.ContatoEmergenciaNome,
                cliente.ContatoEmergenciaCelular,
                preenchidosEmUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoPreenchimentoDadosPessoais(
            StatusPreenchimentoDadosPessoais.Preenchidos,
            ficha.Id,
            cliente.Id,
            dadosDaFicha.NomeParaExibicao,
            preenchidosEmUtc);
    }
}

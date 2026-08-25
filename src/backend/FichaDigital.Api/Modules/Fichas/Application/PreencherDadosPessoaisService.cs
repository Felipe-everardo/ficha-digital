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

        if (cliente.DadosPessoaisPreenchidos)
        {
            return new ResultadoPreenchimentoDadosPessoais(
                StatusPreenchimentoDadosPessoais.JaPreenchidos);
        }

        var preenchidosEmUtc = timeProvider.GetUtcNow();
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

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoPreenchimentoDadosPessoais(
            StatusPreenchimentoDadosPessoais.Preenchidos,
            ficha.Id,
            cliente.Id,
            cliente.NomeParaExibicao,
            preenchidosEmUtc);
    }
}

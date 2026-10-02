using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Shared.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class PreencherDadosPessoaisService(
    IDadosPessoaisRepository repository,
    IGeradorTokenConvite geradorToken,
    TimeProvider timeProvider)
{
    public async Task<ResultadoPreenchimentoDadosPessoais> PreencherAsync(
        PreencherDadosPessoaisCommand command,
        string correlacaoId,
        CancellationToken cancellationToken)
    {
        var tokenHash = geradorToken.CalcularHash(command.TokenOriginal);
        var convite = await repository.ObterConviteAsync(tokenHash, cancellationToken);

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

        var ficha = await repository.ObterFichaAsync(convite.FichaId, cancellationToken);

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

        var cliente = await repository.ObterClienteAsync(ficha.ClienteId, cancellationToken);

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

        var dadosDaFicha = await repository.ObterDadosPessoaisAsync(ficha.Id, cancellationToken);

        if (dadosDaFicha is null)
        {
            dadosDaFicha = new DadosPessoaisFicha(
                ficha.Id,
                dadosInformados,
                preenchidosEmUtc);
        }
        else
        {
            dadosDaFicha.Atualizar(
                dadosInformados,
                preenchidosEmUtc);
        }

        await repository.SalvarConfirmacaoAsync(ficha, dadosDaFicha, correlacaoId, cancellationToken);

        return new ResultadoPreenchimentoDadosPessoais(
            StatusPreenchimentoDadosPessoais.Preenchidos,
            ficha.Id,
            cliente.Id,
            dadosDaFicha.NomeParaExibicao,
            preenchidosEmUtc);
    }
}

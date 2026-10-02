using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IAberturaConviteRepository
{
    Task<ConviteFicha?> ObterConviteAsync(string tokenHash, CancellationToken cancellationToken);

    Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<QuestionarioSaude?> ObterQuestionarioAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<DadosPessoaisFicha?> ObterDadosPessoaisAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<Cliente?> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken);

    // Persiste os registros e alterações das entidades obtidas nesta unidade de trabalho.
    // A auditoria, quando aplicável, participa da mesma gravação.
    Task SalvarAberturaAsync(Ficha ficha, string correlacaoId, CancellationToken cancellationToken);
}

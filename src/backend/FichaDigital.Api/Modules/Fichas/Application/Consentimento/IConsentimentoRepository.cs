using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IConsentimentoRepository
{
    Task<ConviteFicha?> ObterConviteAsync(string tokenHash, CancellationToken cancellationToken);

    Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<QuestionarioSaude?> ObterQuestionarioAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<DadosPessoaisFicha?> ObterDadosPessoaisAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<Cliente?> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken);

    Task<bool> AceiteExisteAsync(Guid fichaId, CancellationToken cancellationToken);

    // Persiste os registros e alterações das entidades obtidas nesta unidade de trabalho.
    // A auditoria, quando aplicável, participa da mesma gravação.
    Task SalvarAceiteAsync(Ficha ficha, AceiteTermoConsentimento aceite, string correlacaoId, CancellationToken cancellationToken);
}

using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IRevisaoFichaRepository
{
    Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<AceiteTermoConsentimento?> ObterAceiteAsync(Guid fichaId, CancellationToken cancellationToken);

    // Persiste os registros e alterações das entidades obtidas nesta unidade de trabalho.
    // A auditoria, quando aplicável, participa da mesma gravação.
    Task SalvarRevisaoAsync(Ficha ficha, RevisaoProfissional revisao, CancellationToken cancellationToken);
}

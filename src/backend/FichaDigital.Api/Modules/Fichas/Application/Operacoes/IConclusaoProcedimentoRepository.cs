using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IConclusaoProcedimentoRepository
{
    Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken);

    // Persiste os registros e alterações das entidades obtidas nesta unidade de trabalho.
    // A auditoria, quando aplicável, participa da mesma gravação.
    Task SalvarConclusaoAsync(Ficha ficha, RegistroTatuagem? tatuagem, RegistroPiercing? piercing, CancellationToken cancellationToken);
}

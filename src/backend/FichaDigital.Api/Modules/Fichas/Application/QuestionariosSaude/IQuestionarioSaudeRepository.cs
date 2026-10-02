using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IQuestionarioSaudeRepository
{
    Task<ConviteFicha?> ObterConviteAsync(string tokenHash, CancellationToken cancellationToken);

    Task<Ficha?> ObterFichaAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<bool> QuestionarioExisteAsync(Guid fichaId, CancellationToken cancellationToken);

    Task<bool> DadosPessoaisExistemAsync(Guid fichaId, CancellationToken cancellationToken);

    // Persiste os registros e alterações das entidades obtidas nesta unidade de trabalho.
    // A auditoria, quando aplicável, participa da mesma gravação.
    Task SalvarRespostaAsync(Ficha ficha, QuestionarioSaude questionario, string correlacaoId, CancellationToken cancellationToken);
}

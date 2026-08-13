namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class AceiteTermoConsentimento
{
    private const int TamanhoMaximoNomeAssinante = 150;

    private AceiteTermoConsentimento()
    {
    }

    public AceiteTermoConsentimento(
        Guid fichaId,
        int versaoTermo,
        string conteudoTermo,
        string conteudoHash,
        string nomeAssinante,
        DateTimeOffset aceitoEmUtc)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException(
                "A ficha é obrigatória.",
                nameof(fichaId));
        }

        if (versaoTermo <= 0)
        {
            throw new ArgumentException(
                "A versão do termo deve ser maior que zero.",
                nameof(versaoTermo));
        }

        if (string.IsNullOrWhiteSpace(conteudoTermo))
        {
            throw new ArgumentException(
                "O conteúdo do termo é obrigatório.",
                nameof(conteudoTermo));
        }

        if (string.IsNullOrWhiteSpace(conteudoHash) ||
            conteudoHash.Trim().Length != 64)
        {
            throw new ArgumentException(
                "O hash do conteúdo deve possuir 64 caracteres.",
                nameof(conteudoHash));
        }

        if (string.IsNullOrWhiteSpace(nomeAssinante))
        {
            throw new ArgumentException(
                "O nome do assinante é obrigatório.",
                nameof(nomeAssinante));
        }

        if (nomeAssinante.Trim().Length > TamanhoMaximoNomeAssinante)
        {
            throw new ArgumentException(
                "O nome do assinante deve ter no máximo 150 caracteres.",
                nameof(nomeAssinante));
        }

        if (aceitoEmUtc == default)
        {
            throw new ArgumentException(
                "A data do aceite é obrigatória.",
                nameof(aceitoEmUtc));
        }

        Id = Guid.NewGuid();
        FichaId = fichaId;
        VersaoTermo = versaoTermo;
        ConteudoTermo = conteudoTermo.Trim();
        ConteudoHash = conteudoHash.Trim().ToLowerInvariant();
        NomeAssinante = nomeAssinante.Trim();
        AceitoEmUtc = aceitoEmUtc;
    }

    public Guid Id { get; private set; }

    public Guid FichaId { get; private set; }

    public int VersaoTermo { get; private set; }

    public string ConteudoTermo { get; private set; } = string.Empty;

    public string ConteudoHash { get; private set; } = string.Empty;

    public string NomeAssinante { get; private set; } = string.Empty;

    public DateTimeOffset AceitoEmUtc { get; private set; }
}

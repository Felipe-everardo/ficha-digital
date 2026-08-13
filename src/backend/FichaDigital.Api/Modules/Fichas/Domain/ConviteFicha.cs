namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class ConviteFicha
{
    private ConviteFicha()
    {
    }

    public ConviteFicha(
        Guid fichaId,
        string tokenHash,
        DateTimeOffset expiraEmUtc)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException(
                "A ficha é obrigatória.",
                nameof(fichaId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException(
                "O hash do token é obrigatório.",
                nameof(tokenHash));
        }

        var criadaEmUtc = DateTimeOffset.UtcNow;

        if (expiraEmUtc <= criadaEmUtc)
        {
            throw new ArgumentException(
                "A data de expiração deve estar no futuro.",
                nameof(expiraEmUtc));
        }

        Id = Guid.NewGuid();
        FichaId = fichaId;
        TokenHash = tokenHash.Trim();
        CriadoEmUtc = criadaEmUtc;
        ExpiraEmUtc = expiraEmUtc;
    }

    public Guid Id { get; private set; }

    public Guid FichaId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CriadoEmUtc { get; private set; }

    public DateTimeOffset ExpiraEmUtc { get; private set; }

    public bool EstaExpirado(DateTimeOffset instanteUtc)
    {
        return instanteUtc >= ExpiraEmUtc;
    }
}

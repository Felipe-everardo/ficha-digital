namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class RevisaoProfissional
{
    private RevisaoProfissional()
    {
    }

    public RevisaoProfissional(
        Guid fichaId,
        Guid profissionalId,
        string profissionalNome,
        bool dadosDaFichaConferidos,
        DateTimeOffset revisadaEmUtc)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException("A ficha é obrigatória.", nameof(fichaId));
        }

        if (profissionalId == Guid.Empty)
        {
            throw new ArgumentException(
                "O profissional é obrigatório.",
                nameof(profissionalId));
        }

        if (string.IsNullOrWhiteSpace(profissionalNome))
        {
            throw new ArgumentException(
                "O nome do profissional é obrigatório.",
                nameof(profissionalNome));
        }

        if (!dadosDaFichaConferidos)
        {
            throw new ArgumentException(
                "Os dados da ficha precisam ser conferidos antes da confirmação.",
                nameof(dadosDaFichaConferidos));
        }

        if (revisadaEmUtc == default)
        {
            throw new ArgumentException(
                "A data da revisão é obrigatória.",
                nameof(revisadaEmUtc));
        }

        FichaId = fichaId;
        ProfissionalId = profissionalId;
        ProfissionalNome = profissionalNome.Trim();
        DadosDaFichaConferidos = dadosDaFichaConferidos;
        RevisadaEmUtc = revisadaEmUtc;
    }

    public Guid FichaId { get; private set; }

    public Guid ProfissionalId { get; private set; }

    public string ProfissionalNome { get; private set; } = string.Empty;

    public bool DadosDaFichaConferidos { get; private set; }

    public DateTimeOffset RevisadaEmUtc { get; private set; }
}

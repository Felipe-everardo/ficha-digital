namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class Ficha
{
    private Ficha()
    {
    }

    public Ficha(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
        {
            throw new ArgumentException(
                "O cliente é obrigatório.",
                nameof(clienteId));
        }

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Status = StatusFicha.Rascunho;
        CriadaEmUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ClienteId { get; private set; }

    public StatusFicha Status { get; private set; }

    public DateTimeOffset CriadaEmUtc { get; private set; }

    public void EnviarConvite()
    {
        if (Status != StatusFicha.Rascunho)
        {
            throw new InvalidOperationException(
                "Somente uma ficha em rascunho pode ter o convite enviado.");
        }

        Status = StatusFicha.ConviteEnviado;
    }

    public void IniciarPreenchimento()
    {
        if (Status != StatusFicha.ConviteEnviado)
        {
            throw new InvalidOperationException(
                "Somente uma ficha com convite enviado pode iniciar o preenchimento.");
        }

        Status = StatusFicha.EmPreenchimento;
    }

    public void Concluir()
    {
        if (Status != StatusFicha.EmPreenchimento)
        {
            throw new InvalidOperationException(
                "Somente uma ficha em preenchimento pode ser concluída.");
        }

        Status = StatusFicha.Concluida;
    }
}

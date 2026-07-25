namespace FichaDigital.Api.Modules.Clientes.Domain;

public sealed class Cliente
{
    private Cliente()
    {
    }

    public Cliente(string nomeCompleto, string? nomeSocial, string? pronomes, DateOnly dataNascimento, string celular, string? email)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
        {
            throw new ArgumentException(
                "O nome completo é obrigatório.",
                nameof(nomeCompleto));
        }

        if (string.IsNullOrWhiteSpace(celular))
        {
            throw new ArgumentException(
                "O celular é obrigatório.",
                nameof(celular));
        }

        if (dataNascimento > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException(
                "A data de nascimento não pode estar no futuro.",
                nameof(dataNascimento));
        }

        Id = Guid.NewGuid();
        NomeCompleto = nomeCompleto.Trim();
        NomeSocial = string.IsNullOrWhiteSpace(nomeSocial) ? null : nomeSocial.Trim();
        Pronomes = string.IsNullOrWhiteSpace(pronomes) ? null : pronomes.Trim();
        DataNascimento = dataNascimento;
        Celular = celular.Trim();
        Email = string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim();
        CriadoEmUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string NomeCompleto { get; private set; } = string.Empty;

    public string? NomeSocial { get; private set; }

    public string NomeParaExibicao => NomeSocial ?? NomeCompleto;

    public string? Pronomes { get; private set; }

    public DateOnly DataNascimento { get; private set; }

    public string Celular { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public DateTimeOffset CriadoEmUtc { get; private set; }


    public void AtualizarContato(string celular, string? email)
    {
        Celular = !string.IsNullOrWhiteSpace(celular) ? celular.Trim() : throw new ArgumentException("O celular é obrigatório.", nameof(celular));
        Email = !string.IsNullOrWhiteSpace(email) ? email.Trim() : null;
    }
}

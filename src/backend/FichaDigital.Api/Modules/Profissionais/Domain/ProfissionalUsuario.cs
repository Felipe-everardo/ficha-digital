using Microsoft.AspNetCore.Identity;

namespace FichaDigital.Api.Modules.Profissionais.Domain;

public sealed class ProfissionalUsuario : IdentityUser<Guid>
{
    private ProfissionalUsuario()
    {
    }

    public ProfissionalUsuario(
        string nomeCompleto,
        string email,
        EspecialidadesProfissional especialidades =
            EspecialidadesProfissional.Nenhuma)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
        {
            throw new ArgumentException(
                "O nome completo é obrigatório.",
                nameof(nomeCompleto));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "O e-mail é obrigatório.",
                nameof(email));
        }

        Id = Guid.NewGuid();
        NomeCompleto = nomeCompleto.Trim();
        Especialidades = especialidades;
        Email = email.Trim();
        UserName = Email;
    }

    public string NomeCompleto { get; private set; } = string.Empty;

    public EspecialidadesProfissional Especialidades { get; private set; }

    public void AtualizarEspecialidades(
        EspecialidadesProfissional especialidades)
    {
        Especialidades = especialidades;
    }
}

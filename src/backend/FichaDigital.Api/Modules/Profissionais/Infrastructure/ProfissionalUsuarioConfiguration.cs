using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Profissionais.Infrastructure;

public sealed class ProfissionalUsuarioConfiguration
    : IEntityTypeConfiguration<ProfissionalUsuario>
{
    public void Configure(
        EntityTypeBuilder<ProfissionalUsuario> builder)
    {
        builder.Property(profissional => profissional.NomeCompleto)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(profissional => profissional.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(profissional => profissional.NormalizedEmail)
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(profissional => profissional.UserName)
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(profissional => profissional.NormalizedUserName)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(profissional => profissional.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique();
    }
}

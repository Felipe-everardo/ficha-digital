using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class AceiteTermoConsentimentoConfiguration
    : IEntityTypeConfiguration<AceiteTermoConsentimento>
{
    public void Configure(
        EntityTypeBuilder<AceiteTermoConsentimento> builder)
    {
        builder.ToTable("AceitesTermoConsentimento");

        builder.HasKey(aceite => aceite.Id);

        builder.Property(aceite => aceite.Id)
            .ValueGeneratedNever();

        builder.Property(aceite => aceite.FichaId)
            .IsRequired();

        builder.Property(aceite => aceite.VersaoTermo)
            .IsRequired();

        builder.Property(aceite => aceite.ConteudoTermo)
            .IsRequired();

        builder.Property(aceite => aceite.ConteudoHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(aceite => aceite.NomeAssinante)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(aceite => aceite.AceitoEmUtc)
            .IsRequired();

        builder.HasOne<Ficha>()
            .WithOne()
            .HasForeignKey<AceiteTermoConsentimento>(
                aceite => aceite.FichaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(aceite => aceite.FichaId)
            .IsUnique();
    }
}

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

        builder.Property(aceite => aceite.ConviteId);

        builder.Property(aceite => aceite.ConfirmouLeituraEAutorizacao)
            .IsRequired();

        builder.Property(aceite => aceite.VersaoEvidencia)
            .IsRequired();

        builder.Property(aceite => aceite.EvidenciaJson)
            .IsRequired();

        builder.Property(aceite => aceite.EvidenciaHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(aceite => aceite.EnderecoIp)
            .HasMaxLength(64);

        builder.Property(aceite => aceite.AgenteUsuario)
            .HasMaxLength(512);

        builder.Property(aceite => aceite.AceitoEmUtc)
            .IsRequired();

        builder.Property(aceite => aceite.AssinaturaDesenhada);

        builder.HasOne<Ficha>()
            .WithOne()
            .HasForeignKey<AceiteTermoConsentimento>(
                aceite => aceite.FichaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ConviteFicha>()
            .WithMany()
            .HasForeignKey(aceite => aceite.ConviteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(aceite => aceite.FichaId)
            .IsUnique();

        builder.HasIndex(aceite => aceite.ConviteId);
    }
}

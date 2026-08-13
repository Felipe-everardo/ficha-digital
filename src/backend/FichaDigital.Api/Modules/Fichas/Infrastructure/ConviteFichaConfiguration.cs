using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class ConviteFichaConfiguration
    : IEntityTypeConfiguration<ConviteFicha>
{
    public void Configure(EntityTypeBuilder<ConviteFicha> builder)
    {
        builder.ToTable("ConvitesFicha");

        builder.HasKey(convite => convite.Id);

        builder.Property(convite => convite.Id)
            .ValueGeneratedNever();

        builder.Property(convite => convite.FichaId)
            .IsRequired();

        builder.Property(convite => convite.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(convite => convite.CriadoEmUtc)
            .IsRequired();

        builder.Property(convite => convite.ExpiraEmUtc)
            .IsRequired();

        builder.HasOne<Ficha>()
            .WithMany()
            .HasForeignKey(convite => convite.FichaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(convite => convite.FichaId);

        builder.HasIndex(convite => convite.TokenHash)
            .IsUnique();
    }
}

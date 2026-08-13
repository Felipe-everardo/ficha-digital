using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class FichaConfiguration : IEntityTypeConfiguration<Ficha>
{
    public void Configure(EntityTypeBuilder<Ficha> builder)
    {
        builder.ToTable("Fichas");

        builder.HasKey(ficha => ficha.Id);

        builder.Property(ficha => ficha.Id)
            .ValueGeneratedNever();

        builder.Property(ficha => ficha.ClienteId)
            .IsRequired();

        builder.Property(ficha => ficha.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(ficha => ficha.CriadaEmUtc)
            .IsRequired();

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(ficha => ficha.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ficha => ficha.ClienteId);
    }
}

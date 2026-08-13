using FichaDigital.Api.Modules.Clientes.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Clientes.Infrastructure;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Id)
            .ValueGeneratedNever();

        builder.Property(cliente => cliente.NomeCompleto)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(cliente => cliente.NomeSocial)
            .HasMaxLength(150);

        builder.Ignore(cliente => cliente.NomeParaExibicao);

        builder.Property(cliente => cliente.Pronomes)
            .HasMaxLength(50);

        builder.Property(cliente => cliente.DataNascimento)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(cliente => cliente.Celular)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(cliente => cliente.Email)
            .HasMaxLength(254);

        builder.Property(cliente => cliente.CriadoEmUtc)
            .IsRequired();

        builder.HasIndex(cliente => cliente.NomeCompleto);

        builder.HasIndex(cliente => cliente.Celular);
    }
}

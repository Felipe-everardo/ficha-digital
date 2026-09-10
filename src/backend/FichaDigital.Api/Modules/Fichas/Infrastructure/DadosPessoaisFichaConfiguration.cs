using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class DadosPessoaisFichaConfiguration
    : IEntityTypeConfiguration<DadosPessoaisFicha>
{
    public void Configure(EntityTypeBuilder<DadosPessoaisFicha> builder)
    {
        builder.ToTable("DadosPessoaisFichas");

        builder.HasKey(dados => dados.FichaId);

        builder.Property(dados => dados.FichaId)
            .ValueGeneratedNever();

        builder.Property(dados => dados.NomeCompleto)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(dados => dados.NomeSocial)
            .HasMaxLength(150);

        builder.Property(dados => dados.Pronomes)
            .HasMaxLength(50);

        builder.Property(dados => dados.DataNascimento)
            .IsRequired();

        builder.Property(dados => dados.Celular)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(dados => dados.Email)
            .HasMaxLength(254);

        builder.Property(dados => dados.Instagram)
            .HasMaxLength(100);

        builder.Property(dados => dados.ContatoEmergenciaNome)
            .HasMaxLength(150);

        builder.Property(dados => dados.ContatoEmergenciaCelular)
            .HasMaxLength(25);

        builder.Property(dados => dados.ConfirmadosEmUtc)
            .IsRequired();

        builder.Ignore(dados => dados.NomeParaExibicao);

        builder.HasOne<Ficha>()
            .WithOne()
            .HasForeignKey<DadosPessoaisFicha>(dados => dados.FichaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

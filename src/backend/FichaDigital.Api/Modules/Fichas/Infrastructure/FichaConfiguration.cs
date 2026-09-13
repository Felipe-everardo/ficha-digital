using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
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

        builder.Property(ficha => ficha.ProfissionalResponsavelNome)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(ficha => ficha.TipoProcedimento)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(ficha => ficha.VersaoModelo);

        builder.Property(ficha => ficha.VersaoQuestionario);

        builder.Property(ficha => ficha.VersaoTermo);

        builder.Property(ficha => ficha.CnpjApresentado)
            .HasMaxLength(18);

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

        builder.HasOne<ProfissionalUsuario>()
            .WithMany()
            .HasForeignKey(ficha => ficha.ProfissionalResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ficha => ficha.ProfissionalResponsavelId);

        builder.HasIndex(ficha => ficha.TipoProcedimento);

        builder.HasIndex(ficha => ficha.Status);
    }
}

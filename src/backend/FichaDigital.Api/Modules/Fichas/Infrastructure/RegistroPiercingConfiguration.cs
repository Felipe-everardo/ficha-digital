using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class RegistroPiercingConfiguration
    : IEntityTypeConfiguration<RegistroPiercing>
{
    public void Configure(EntityTypeBuilder<RegistroPiercing> builder)
    {
        builder.ToTable("RegistrosPiercing");
        builder.HasKey(registro => registro.FichaId);
        builder.Property(registro => registro.FichaId).ValueGeneratedNever();
        builder.Property(registro => registro.ProfissionalNome)
            .HasMaxLength(150).IsRequired();
        builder.Property(registro => registro.JoiaUtilizada)
            .HasMaxLength(500).IsRequired();
        builder.Property(registro => registro.AgulhaUtilizada)
            .HasMaxLength(500).IsRequired();
        builder.Property(registro => registro.LocalPerfuracao)
            .HasMaxLength(300).IsRequired();
        builder.Property(registro => registro.Observacoes).HasMaxLength(2000);
        builder.Property(registro => registro.ValorTotal)
            .HasPrecision(12, 2).IsRequired();
        builder.Property(registro => registro.ValorSinal)
            .HasPrecision(12, 2).IsRequired();
        builder.Property(registro => registro.FormaPagamento)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(registro => registro.NomeProfissionalAssinante)
            .HasMaxLength(150).IsRequired();
        builder.Property(registro => registro.AssinaturaDesenhada).IsRequired();
        builder.Property(registro => registro.EvidenciaJson).IsRequired();
        builder.Property(registro => registro.EvidenciaHash)
            .HasMaxLength(64).IsRequired();
        builder.HasOne<Ficha>().WithOne()
            .HasForeignKey<RegistroPiercing>(registro => registro.FichaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

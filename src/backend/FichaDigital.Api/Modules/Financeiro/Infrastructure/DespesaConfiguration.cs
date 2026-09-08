using FichaDigital.Api.Modules.Financeiro.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Financeiro.Infrastructure;

public sealed class DespesaConfiguration : IEntityTypeConfiguration<Despesa>
{
    public void Configure(EntityTypeBuilder<Despesa> builder)
    {
        builder.ToTable("Despesas");

        builder.HasKey(despesa => despesa.Id);

        builder.Property(despesa => despesa.Id)
            .ValueGeneratedNever();

        builder.Property(despesa => despesa.Data)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(despesa => despesa.Categoria)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(despesa => despesa.Descricao)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(despesa => despesa.Valor)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(despesa => despesa.ProfissionalNome)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(despesa => despesa.RegistradaEmUtc)
            .IsRequired();

        builder.Property(despesa => despesa.AtualizadaEmUtc)
            .IsRequired();

        builder.HasOne<ProfissionalUsuario>()
            .WithMany()
            .HasForeignKey(despesa => despesa.ProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(despesa => despesa.Data);

        builder.HasIndex(despesa => despesa.Categoria);

        builder.HasIndex(despesa => despesa.ProfissionalId);
    }
}

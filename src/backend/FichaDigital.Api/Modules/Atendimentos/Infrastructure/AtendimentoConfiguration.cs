using FichaDigital.Api.Modules.Atendimentos.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Atendimentos.Infrastructure;

public sealed class AtendimentoConfiguration
    : IEntityTypeConfiguration<Atendimento>
{
    public void Configure(EntityTypeBuilder<Atendimento> builder)
    {
        builder.ToTable("Atendimentos");

        builder.HasKey(atendimento => atendimento.Id);

        builder.Property(atendimento => atendimento.Id)
            .ValueGeneratedNever();

        builder.Property(atendimento => atendimento.DataRealizacao)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(atendimento => atendimento.ValorCobrado)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(atendimento => atendimento.Desconto)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(atendimento => atendimento.ValorFinal)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(atendimento => atendimento.FormaPagamento)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(atendimento => atendimento.SituacaoPagamento)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(atendimento => atendimento.RegistradoEmUtc)
            .IsRequired();

        builder.Property(atendimento => atendimento.AtualizadoEmUtc)
            .IsRequired();

        builder.HasOne<Ficha>()
            .WithOne()
            .HasForeignKey<Atendimento>(atendimento => atendimento.FichaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(atendimento => atendimento.FichaId)
            .IsUnique();

        builder.HasIndex(atendimento => atendimento.DataRealizacao);

        builder.HasIndex(atendimento => atendimento.SituacaoPagamento);
    }
}

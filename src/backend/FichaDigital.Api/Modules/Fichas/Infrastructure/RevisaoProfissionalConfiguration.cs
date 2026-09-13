using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class RevisaoProfissionalConfiguration
    : IEntityTypeConfiguration<RevisaoProfissional>
{
    public void Configure(EntityTypeBuilder<RevisaoProfissional> builder)
    {
        builder.ToTable("RevisoesProfissionais");
        builder.HasKey(revisao => revisao.FichaId);
        builder.Property(revisao => revisao.FichaId).ValueGeneratedNever();
        builder.Property(revisao => revisao.ProfissionalNome)
            .HasMaxLength(150).IsRequired();
        builder.HasOne<Ficha>().WithOne()
            .HasForeignKey<RevisaoProfissional>(revisao => revisao.FichaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

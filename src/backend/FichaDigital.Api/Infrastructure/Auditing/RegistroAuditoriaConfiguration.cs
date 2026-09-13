using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Infrastructure.Auditing;

public sealed class RegistroAuditoriaConfiguration
    : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("RegistrosAuditoria");
        builder.HasKey(registro => registro.Id);
        builder.Property(registro => registro.Id).ValueGeneratedNever();
        builder.Property(registro => registro.Acao)
            .HasMaxLength(160)
            .IsRequired();
        builder.Property(registro => registro.Origem)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(registro => registro.Recurso)
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(registro => registro.CorrelacaoId)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(registro => registro.OcorreuEmUtc).IsRequired();
        builder.HasIndex(registro => registro.ProfissionalId);
        builder.HasIndex(registro => new
        {
            registro.Recurso,
            registro.RecursoId,
            registro.OcorreuEmUtc
        });
    }
}

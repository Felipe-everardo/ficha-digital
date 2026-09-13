using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Infrastructure.Idempotency;

public sealed class RequisicaoIdempotenteConfiguration
    : IEntityTypeConfiguration<RequisicaoIdempotente>
{
    public void Configure(EntityTypeBuilder<RequisicaoIdempotente> builder)
    {
        builder.ToTable("RequisicoesIdempotentes");
        builder.HasKey(requisicao => requisicao.Id);
        builder.Property(requisicao => requisicao.Id).ValueGeneratedNever();
        builder.Property(requisicao => requisicao.Escopo)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(requisicao => requisicao.Chave)
            .HasMaxLength(80)
            .IsRequired();
        builder.Property(requisicao => requisicao.Fingerprint)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(requisicao => requisicao.Metodo)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(requisicao => requisicao.Caminho)
            .HasMaxLength(300)
            .IsRequired();
        builder.Property(requisicao => requisicao.TipoConteudo)
            .HasMaxLength(150);
        builder.Property(requisicao => requisicao.Localizacao)
            .HasMaxLength(500);
        builder.HasIndex(requisicao => new
        {
            requisicao.Escopo,
            requisicao.Chave
        }).IsUnique();
        builder.HasIndex(requisicao => requisicao.ExpiraEmUtc);
    }
}

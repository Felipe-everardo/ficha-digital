using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class QuestionarioSaudeConfiguration
    : IEntityTypeConfiguration<QuestionarioSaude>
{
    public void Configure(EntityTypeBuilder<QuestionarioSaude> builder)
    {
        builder.ToTable("QuestionariosSaude");

        builder.HasKey(questionario => questionario.Id);

        builder.Property(questionario => questionario.Id)
            .ValueGeneratedNever();

        builder.Property(questionario => questionario.FichaId)
            .IsRequired();

        builder.Property(questionario => questionario.Versao)
            .IsRequired();

        builder.Property(questionario => questionario.TemDiabetes)
            .IsRequired();

        builder.Property(questionario => questionario.TipoDiabetes)
            .HasMaxLength(100);

        builder.Property(questionario => questionario.TeveAnemia)
            .IsRequired();

        builder.Property(questionario => questionario.DescricaoAnemia)
            .HasMaxLength(300);

        builder.Property(questionario => questionario.TeveHepatite)
            .IsRequired();

        builder.Property(questionario => questionario.TipoHepatite)
            .HasMaxLength(100);

        builder.Property(questionario => questionario.PossuiPressaoAlta)
            .IsRequired();

        builder.Property(questionario => questionario.TemAlergia)
            .IsRequired();

        builder.Property(questionario => questionario.DescricaoAlergia)
            .HasMaxLength(300);

        builder.Property(questionario => questionario.PossuiCondicaoCardiaca)
            .IsRequired();

        builder.Property(questionario => questionario.TemEpilepsia)
            .IsRequired();

        builder.Property(questionario => questionario.TemHemofilia)
            .IsRequired();

        builder.Property(questionario => questionario.PossuiDoencaTransmissivel)
            .IsRequired();

        builder.Property(questionario => questionario.DescricaoDoencaTransmissivel)
            .HasMaxLength(300);

        builder.Property(questionario => questionario.UsaMarcaPasso)
            .IsRequired();

        builder.Property(questionario => questionario.Fuma)
            .IsRequired();

        builder.Property(
                questionario => questionario.ConsumiuBebidaAlcoolicaUltimas24Horas)
            .IsRequired();

        builder.Property(questionario => questionario.UsaMedicacao)
            .IsRequired();

        builder.Property(questionario => questionario.DescricaoMedicacao)
            .HasMaxLength(300);

        builder.Property(
                questionario => questionario.EstaGravidaOuAmamentando)
            .IsRequired();

        builder.Property(questionario => questionario.RespondidoEmUtc)
            .IsRequired();

        builder.HasOne<Ficha>()
            .WithOne()
            .HasForeignKey<QuestionarioSaude>(
                questionario => questionario.FichaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(questionario => questionario.FichaId)
            .IsUnique();
    }
}

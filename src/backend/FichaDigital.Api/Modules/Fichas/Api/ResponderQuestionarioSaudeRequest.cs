using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class ResponderQuestionarioSaudeRequest : IValidatableObject
{
    [Required(ErrorMessage = "O token é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O token deve ter no máximo 100 caracteres.")]
    public string Token { get; init; } = string.Empty;

    [Required(ErrorMessage = "A resposta sobre diabetes é obrigatória.")]
    public bool? TemDiabetes { get; init; }

    [MaxLength(100, ErrorMessage = "O tipo de diabetes deve ter no máximo 100 caracteres.")]
    public string? TipoDiabetes { get; init; }

    [Required(ErrorMessage = "A resposta sobre pressão alta é obrigatória.")]
    public bool? PossuiPressaoAlta { get; init; }

    [Required(ErrorMessage = "A resposta sobre alergias é obrigatória.")]
    public bool? TemAlergia { get; init; }

    [MaxLength(300, ErrorMessage = "A descrição da alergia deve ter no máximo 300 caracteres.")]
    public string? DescricaoAlergia { get; init; }

    [Required(ErrorMessage = "A resposta sobre condição cardíaca é obrigatória.")]
    public bool? PossuiCondicaoCardiaca { get; init; }

    [Required(ErrorMessage = "A resposta sobre epilepsia é obrigatória.")]
    public bool? TemEpilepsia { get; init; }

    [Required(ErrorMessage = "A resposta sobre hemofilia é obrigatória.")]
    public bool? TemHemofilia { get; init; }

    [Required(ErrorMessage = "A resposta sobre uso de marca-passo é obrigatória.")]
    public bool? UsaMarcaPasso { get; init; }

    [Required(ErrorMessage = "A resposta sobre gestação ou amamentação é obrigatória.")]
    public bool? EstaGravidaOuAmamentando { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (TemDiabetes is true &&
            string.IsNullOrWhiteSpace(TipoDiabetes))
        {
            yield return new ValidationResult(
                "O tipo de diabetes é obrigatório quando a resposta for sim.",
                [nameof(TipoDiabetes)]);
        }

        if (TemAlergia is true &&
            string.IsNullOrWhiteSpace(DescricaoAlergia))
        {
            yield return new ValidationResult(
                "A descrição da alergia é obrigatória quando a resposta for sim.",
                [nameof(DescricaoAlergia)]);
        }
    }
}

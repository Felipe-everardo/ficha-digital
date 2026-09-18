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

    [Required(ErrorMessage = "A resposta sobre anemia é obrigatória.")]
    public bool? TeveAnemia { get; init; }

    [MaxLength(300, ErrorMessage = "Os detalhes sobre a anemia devem ter no máximo 300 caracteres.")]
    public string? DescricaoAnemia { get; init; }

    [Required(ErrorMessage = "A resposta sobre hepatite é obrigatória.")]
    public bool? TeveHepatite { get; init; }

    [MaxLength(100, ErrorMessage = "O tipo de hepatite deve ter no máximo 100 caracteres.")]
    public string? TipoHepatite { get; init; }

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

    [Required(ErrorMessage = "A resposta sobre doença transmissível é obrigatória.")]
    public bool? PossuiDoencaTransmissivel { get; init; }

    [MaxLength(300, ErrorMessage = "A descrição da doença transmissível deve ter no máximo 300 caracteres.")]
    public string? DescricaoDoencaTransmissivel { get; init; }

    [Required(ErrorMessage = "A resposta sobre uso de marca-passo é obrigatória.")]
    public bool? UsaMarcaPasso { get; init; }

    [Required(ErrorMessage = "A resposta sobre tabagismo é obrigatória.")]
    public bool? Fuma { get; init; }

    [Required(ErrorMessage = "A resposta sobre consumo de álcool é obrigatória.")]
    public bool? ConsumiuBebidaAlcoolicaUltimas24Horas { get; init; }

    [Required(ErrorMessage = "A resposta sobre uso de medicação é obrigatória.")]
    public bool? UsaMedicacao { get; init; }

    [MaxLength(300, ErrorMessage = "A descrição da medicação deve ter no máximo 300 caracteres.")]
    public string? DescricaoMedicacao { get; init; }

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

        if (TeveAnemia is true &&
            string.IsNullOrWhiteSpace(DescricaoAnemia))
        {
            yield return new ValidationResult(
                "Os detalhes sobre a anemia são obrigatórios quando a resposta for sim.",
                [nameof(DescricaoAnemia)]);
        }

        if (TeveHepatite is true &&
            string.IsNullOrWhiteSpace(TipoHepatite))
        {
            yield return new ValidationResult(
                "O tipo de hepatite é obrigatório quando a resposta for sim.",
                [nameof(TipoHepatite)]);
        }

        if (PossuiDoencaTransmissivel is true &&
            string.IsNullOrWhiteSpace(DescricaoDoencaTransmissivel))
        {
            yield return new ValidationResult(
                "A doença transmissível é obrigatória quando a resposta for sim.",
                [nameof(DescricaoDoencaTransmissivel)]);
        }

        if (UsaMedicacao is true &&
            string.IsNullOrWhiteSpace(DescricaoMedicacao))
        {
            yield return new ValidationResult(
                "A medicação utilizada é obrigatória quando a resposta for sim.",
                [nameof(DescricaoMedicacao)]);
        }
    }
}

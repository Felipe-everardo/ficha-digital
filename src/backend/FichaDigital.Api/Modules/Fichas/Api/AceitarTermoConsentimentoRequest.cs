using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class AceitarTermoConsentimentoRequest : IValidatableObject
{
    [Required(ErrorMessage = "O token é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O token deve ter no máximo 100 caracteres.")]
    public string Token { get; init; } = string.Empty;

    [Required(ErrorMessage = "A versão do termo é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A versão do termo é inválida.")]
    public int? VersaoTermo { get; init; }

    [Required(ErrorMessage = "O hash do conteúdo é obrigatório.")]
    [RegularExpression(
        "^[a-fA-F0-9]{64}$",
        ErrorMessage = "O hash do conteúdo é inválido.")]
    public string ConteudoHash { get; init; } = string.Empty;

    [Required(ErrorMessage = "O nome do assinante é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O nome do assinante deve ter no máximo 150 caracteres.")]
    public string NomeAssinante { get; init; } = string.Empty;

    [Required(ErrorMessage = "A confirmação do aceite é obrigatória.")]
    public bool? AceitouTermo { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (AceitouTermo is false)
        {
            yield return new ValidationResult(
                "É necessário aceitar o termo para concluir a ficha.",
                [nameof(AceitouTermo)]);
        }
    }
}

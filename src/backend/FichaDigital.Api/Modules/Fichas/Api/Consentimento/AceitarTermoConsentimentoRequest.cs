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

    [Required(ErrorMessage = "A confirmação de leitura e autorização é obrigatória.")]
    public bool? ConfirmouLeituraEAutorizacao { get; init; }

    [MaxLength(
        global::FichaDigital.Api.Modules.Fichas.Domain.AssinaturaDesenhada
            .TamanhoMaximo,
        ErrorMessage = "A assinatura desenhada excede o tamanho permitido.")]
    public string? AssinaturaDesenhada { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (ConfirmouLeituraEAutorizacao is false)
        {
            yield return new ValidationResult(
                "Confirme que leu, entendeu e autoriza o procedimento.",
                [nameof(ConfirmouLeituraEAutorizacao)]);
        }
    }
}

using System.ComponentModel.DataAnnotations;
using FichaDigital.Api.Modules.Financeiro.Domain;

namespace FichaDigital.Api.Modules.Financeiro.Api;

public sealed class SalvarDespesaRequest : IValidatableObject
{
    [Required(ErrorMessage = "A data da despesa é obrigatória.")]
    public DateOnly? Data { get; init; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public CategoriaDespesa? Categoria { get; init; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(
        200,
        ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
    public string Descricao { get; init; } = string.Empty;

    public decimal Valor { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Categoria is not null && !Enum.IsDefined(Categoria.Value))
        {
            yield return new ValidationResult(
                "A categoria da despesa é inválida.",
                [nameof(Categoria)]);
        }

        if (Valor <= 0 || Valor > 999_999.99m)
        {
            yield return new ValidationResult(
                "O valor deve estar entre 0,01 e 999.999,99.",
                [nameof(Valor)]);
        }

        if (decimal.Round(Valor, 2) != Valor)
        {
            yield return new ValidationResult(
                "O valor deve ter no máximo duas casas decimais.",
                [nameof(Valor)]);
        }
    }
}

using System.ComponentModel.DataAnnotations;
using FichaDigital.Api.Modules.Atendimentos.Domain;

namespace FichaDigital.Api.Modules.Atendimentos.Api;

public sealed class RegistrarAtendimentoRequest : IValidatableObject
{
    [Required(ErrorMessage = "A data do procedimento é obrigatória.")]
    public DateOnly? DataRealizacao { get; init; }

    public decimal ValorCobrado { get; init; }

    public decimal Desconto { get; init; }

    [Required(ErrorMessage = "A forma de pagamento é obrigatória.")]
    public FormaPagamento? FormaPagamento { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (ValorCobrado < 0 || ValorCobrado > 999_999.99m)
        {
            yield return new ValidationResult(
                "O valor cobrado deve estar entre 0 e 999.999,99.",
                [nameof(ValorCobrado)]);
        }

        if (Desconto < 0 || Desconto > 999_999.99m)
        {
            yield return new ValidationResult(
                "O desconto deve estar entre 0 e 999.999,99.",
                [nameof(Desconto)]);
        }

        if (Desconto > ValorCobrado)
        {
            yield return new ValidationResult(
                "O desconto não pode ser maior que o valor cobrado.",
                [nameof(Desconto)]);
        }

        if (decimal.Round(ValorCobrado, 2) != ValorCobrado)
        {
            yield return new ValidationResult(
                "O valor cobrado deve ter no máximo duas casas decimais.",
                [nameof(ValorCobrado)]);
        }

        if (decimal.Round(Desconto, 2) != Desconto)
        {
            yield return new ValidationResult(
                "O desconto deve ter no máximo duas casas decimais.",
                [nameof(Desconto)]);
        }

        if (FormaPagamento is not null &&
            !Enum.IsDefined(FormaPagamento.Value))
        {
            yield return new ValidationResult(
                "A forma de pagamento é inválida.",
                [nameof(FormaPagamento)]);
        }

    }
}

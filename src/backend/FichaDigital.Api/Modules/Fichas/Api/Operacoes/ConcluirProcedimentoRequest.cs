using System.ComponentModel.DataAnnotations;
using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class ConcluirProcedimentoRequest : IValidatableObject
{
    [Required]
    public decimal? ValorTotal { get; init; }

    [Required]
    public decimal? ValorSinal { get; init; }

    [Required]
    public FormaPagamento? FormaPagamento { get; init; }

    [Required]
    [MaxLength(
        global::FichaDigital.Api.Modules.Fichas.Domain.AssinaturaDesenhada
            .TamanhoMaximo)]
    public string AssinaturaDesenhada { get; init; } = string.Empty;

    public RegistroTatuagemRequest? Tatuagem { get; init; }

    public RegistroPiercingRequest? Piercing { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        const decimal valorMaximo = 9_999_999_999.99m;

        if (ValorTotal is < 0 or > valorMaximo)
        {
            yield return new ValidationResult(
                "O valor total informado é inválido.",
                [nameof(ValorTotal)]);
        }

        if (ValorSinal is < 0 or > valorMaximo)
        {
            yield return new ValidationResult(
                "O valor do sinal informado é inválido.",
                [nameof(ValorSinal)]);
        }

        if (ValorTotal is not null &&
            ValorSinal is not null &&
            ValorSinal > ValorTotal)
        {
            yield return new ValidationResult(
                "O sinal não pode ser maior que o valor total.",
                [nameof(ValorSinal)]);
        }
    }
}

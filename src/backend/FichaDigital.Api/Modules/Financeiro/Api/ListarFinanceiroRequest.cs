using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Financeiro.Api;

public sealed class ListarFinanceiroRequest : IValidatableObject
{
    public DateOnly? DataDe { get; init; }

    public DateOnly? DataAte { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que zero.")]
    public int Pagina { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; init; } = 10;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (DataDe is not null &&
            DataAte is not null &&
            DataDe > DataAte)
        {
            yield return new ValidationResult(
                "A data inicial não pode ser posterior à data final.",
                [nameof(DataDe), nameof(DataAte)]);
        }
    }
}

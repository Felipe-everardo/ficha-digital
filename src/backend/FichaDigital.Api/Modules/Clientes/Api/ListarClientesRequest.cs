using System.ComponentModel.DataAnnotations;
using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed class ListarClientesRequest : IValidatableObject
{
    [MaxLength(150, ErrorMessage = "A busca deve ter no máximo 150 caracteres.")]
    public string? Busca { get; init; }

    [MaxLength(25, ErrorMessage = "O telefone deve ter no máximo 25 caracteres.")]
    public string? Telefone { get; init; }

    [MaxLength(100, ErrorMessage = "O Instagram deve ter no máximo 100 caracteres.")]
    public string? Instagram { get; init; }

    public TipoProcedimento? TipoProcedimento { get; init; }

    public DateOnly? UltimaFichaDe { get; init; }

    public DateOnly? UltimaFichaAte { get; init; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "A página deve ser maior ou igual a 1.")]
    public int Pagina { get; init; } = 1;

    [Range(
        1,
        50,
        ErrorMessage = "O tamanho da página deve estar entre 1 e 50.")]
    public int TamanhoPagina { get; init; } = 10;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (UltimaFichaDe > UltimaFichaAte)
        {
            yield return new ValidationResult(
                "A data inicial da ficha não pode ser posterior à data final.",
                [nameof(UltimaFichaDe), nameof(UltimaFichaAte)]);
        }

        if (TipoProcedimento is not null &&
            !Enum.IsDefined(TipoProcedimento.Value))
        {
            yield return new ValidationResult(
                "O procedimento informado é inválido.",
                [nameof(TipoProcedimento)]);
        }
    }
}

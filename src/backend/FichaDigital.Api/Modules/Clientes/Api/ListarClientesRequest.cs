using System.ComponentModel.DataAnnotations;
using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed class ListarClientesRequest : IValidatableObject
{
    [MaxLength(150, ErrorMessage = "A busca deve ter no máximo 150 caracteres.")]
    public string? Busca { get; init; }

    public Guid? ProfissionalId { get; init; }

    public TipoProcedimento? TipoProcedimento { get; init; }

    public DateOnly? AtendimentoDe { get; init; }

    public DateOnly? AtendimentoAte { get; init; }

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
        if (AtendimentoDe > AtendimentoAte)
        {
            yield return new ValidationResult(
                "A data inicial não pode ser posterior à data final.",
                [nameof(AtendimentoDe), nameof(AtendimentoAte)]);
        }

        if (ProfissionalId == Guid.Empty)
        {
            yield return new ValidationResult(
                "O profissional informado é inválido.",
                [nameof(ProfissionalId)]);
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

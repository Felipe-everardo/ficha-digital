using System.ComponentModel.DataAnnotations;
using FichaDigital.Api.Modules.Fichas.Domain;
using TipoProcedimentoFicha =
    FichaDigital.Api.Modules.Fichas.Domain.TipoProcedimento;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class EmitirConviteFichaRequest : IValidatableObject
{
    [Required(ErrorMessage = "O profissional responsável é obrigatório.")]
    [MaxLength(150)]
    public string ProfissionalResponsavelNome { get; init; } = string.Empty;

    [Required(ErrorMessage = "O procedimento é obrigatório.")]
    public TipoProcedimento? TipoProcedimento { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ProfissionalResponsavelNome))
        {
            yield return new ValidationResult(
                "Informe o nome completo do profissional responsável.",
                [nameof(ProfissionalResponsavelNome)]);
        }

        if (TipoProcedimento is null ||
            !Enum.IsDefined(TipoProcedimento.Value) ||
            TipoProcedimento == TipoProcedimentoFicha.NaoInformado)
        {
            yield return new ValidationResult(
                "Selecione Tatuagem ou Piercing.",
                [nameof(TipoProcedimento)]);
        }
    }
}

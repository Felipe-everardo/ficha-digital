using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class RevisarFichaRequest : IValidatableObject
{
    [Required]
    public bool? DadosDaFichaConferidos { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DadosDaFichaConferidos is false)
        {
            yield return new ValidationResult(
                "Confirme a revisão dos dados da ficha.",
                [nameof(DadosDaFichaConferidos)]);
        }
    }
}

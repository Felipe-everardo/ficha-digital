using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class RegistroPiercingRequest
{
    [Required, MaxLength(500)]
    public string JoiaUtilizada { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string AgulhaUtilizada { get; init; } = string.Empty;

    [Required, MaxLength(300)]
    public string LocalPerfuracao { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Observacoes { get; init; }
}

using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class RegistroTatuagemRequest
{
    [Required, MaxLength(500)]
    public string ArteEfetivamenteTatuada { get; init; } = string.Empty;

    [Required, MaxLength(1000)]
    public string MaterialUtilizado { get; init; } = string.Empty;

    [Required, MaxLength(300)]
    public string LocalTatuagem { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Observacoes { get; init; }
}

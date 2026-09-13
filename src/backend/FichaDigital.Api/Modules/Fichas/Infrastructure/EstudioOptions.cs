using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure;

public sealed class EstudioOptions
{
    public const string Secao = "Estudio";

    [Required]
    [MaxLength(18)]
    public string Cnpj { get; init; } = "00.000.000/0000-00";
}

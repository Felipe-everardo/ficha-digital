using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class AbrirConviteFichaRequest
{
    [Required(ErrorMessage = "O token é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O token deve ter no máximo 100 caracteres.")]
    public string Token { get; init; } = string.Empty;
}

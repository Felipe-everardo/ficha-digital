using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Profissionais.Api;

public sealed class EntrarProfissionalRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [MaxLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MaxLength(128, ErrorMessage = "A senha deve ter no máximo 128 caracteres.")]
    public string Senha { get; init; } = string.Empty;
}

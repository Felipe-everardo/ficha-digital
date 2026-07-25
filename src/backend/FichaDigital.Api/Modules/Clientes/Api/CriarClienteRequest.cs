using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed class CriarClienteRequest
{
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O nome completo deve ter no máximo 150 caracteres.")]
    public string NomeCompleto { get; init; } = string.Empty;

    [MaxLength(150, ErrorMessage = "O nome social deve ter no máximo 150 caracteres.")]
    public string? NomeSocial { get; init; }

    [MaxLength(50, ErrorMessage = "Os pronomes devem ter no máximo 50 caracteres.")]
    public string? Pronomes { get; init; }

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    public DateOnly? DataNascimento { get; init; }

    [Required(ErrorMessage = "O celular é obrigatório.")]
    [MaxLength(25, ErrorMessage = "O celular deve ter no máximo 25 caracteres.")]
    public string Celular { get; init; } = string.Empty;

    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [MaxLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    public string? Email { get; init; }
}

using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Clientes.Api;

public sealed class CriarClienteRequest
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O nome do cliente deve ter no máximo 150 caracteres.")]
    public string NomeReferencia { get; init; } = string.Empty;
}

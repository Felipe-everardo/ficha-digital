using System.ComponentModel.DataAnnotations;

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed class PreencherDadosPessoaisRequest : IValidatableObject
{
    [Required(ErrorMessage = "O token é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O token deve ter no máximo 100 caracteres.")]
    public string Token { get; init; } = string.Empty;

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

    [MaxLength(100, ErrorMessage = "O Instagram deve ter no máximo 100 caracteres.")]
    public string? Instagram { get; init; }

    [MaxLength(150, ErrorMessage = "O nome do contato de emergência deve ter no máximo 150 caracteres.")]
    public string? ContatoEmergenciaNome { get; init; }

    [MaxLength(25, ErrorMessage = "O celular do contato de emergência deve ter no máximo 25 caracteres.")]
    public string? ContatoEmergenciaCelular { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (DataNascimento is not null &&
            DataNascimento > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult(
                "A data de nascimento não pode estar no futuro.",
                [nameof(DataNascimento)]);
        }

        var contatoNomeInformado =
            !string.IsNullOrWhiteSpace(ContatoEmergenciaNome);
        var contatoCelularInformado =
            !string.IsNullOrWhiteSpace(ContatoEmergenciaCelular);

        if (contatoNomeInformado != contatoCelularInformado)
        {
            yield return new ValidationResult(
                "Informe o nome e o celular do contato de emergência.",
                [
                    nameof(ContatoEmergenciaNome),
                    nameof(ContatoEmergenciaCelular)
                ]);
        }
    }
}

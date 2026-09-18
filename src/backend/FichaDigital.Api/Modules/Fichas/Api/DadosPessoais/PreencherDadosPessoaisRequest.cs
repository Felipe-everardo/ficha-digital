using System.ComponentModel.DataAnnotations;
using FichaDigital.Api.Shared.Domain;

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

    [Required(ErrorMessage = "O estado civil é obrigatório.")]
    [MaxLength(50, ErrorMessage = "O estado civil deve ter no máximo 50 caracteres.")]
    public string EstadoCivil { get; init; } = string.Empty;

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    public DateOnly? DataNascimento { get; init; }

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [MaxLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres.")]
    public string Cpf { get; init; } = string.Empty;

    [Required(ErrorMessage = "O celular é obrigatório.")]
    [MaxLength(25, ErrorMessage = "O celular deve ter no máximo 25 caracteres.")]
    public string Celular { get; init; } = string.Empty;

    [MaxLength(25, ErrorMessage = "O telefone adicional deve ter no máximo 25 caracteres.")]
    public string? TelefoneAdicional { get; init; }

    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [MaxLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    public string? Email { get; init; }

    [MaxLength(100, ErrorMessage = "O Instagram deve ter no máximo 100 caracteres.")]
    public string? Instagram { get; init; }

    [MaxLength(150, ErrorMessage = "O nome do contato de emergência deve ter no máximo 150 caracteres.")]
    public string? ContatoEmergenciaNome { get; init; }

    [MaxLength(25, ErrorMessage = "O celular do contato de emergência deve ter no máximo 25 caracteres.")]
    public string? ContatoEmergenciaCelular { get; init; }

    [Required(ErrorMessage = "O CEP é obrigatório.")]
    [MaxLength(9, ErrorMessage = "O CEP deve ter no máximo 9 caracteres.")]
    public string Cep { get; init; } = string.Empty;

    [Required(ErrorMessage = "O logradouro é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O logradouro deve ter no máximo 150 caracteres.")]
    public string Logradouro { get; init; } = string.Empty;

    [Required(ErrorMessage = "O número do endereço é obrigatório.")]
    [MaxLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres.")]
    public string Numero { get; init; } = string.Empty;

    [MaxLength(100, ErrorMessage = "O complemento deve ter no máximo 100 caracteres.")]
    public string? Complemento { get; init; }

    [Required(ErrorMessage = "O bairro é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
    public string Bairro { get; init; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [MaxLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
    public string Cidade { get; init; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Informe a sigla do estado com 2 letras.")]
    public string Estado { get; init; } = string.Empty;

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

        if (DataNascimento is null)
        {
            yield break;
        }

        ValidationResult? erroDados = null;

        try
        {
            _ = new DadosPessoaisInformados(
                NomeCompleto,
                NomeSocial,
                Pronomes,
                EstadoCivil,
                DataNascimento.Value,
                Cpf,
                Celular,
                TelefoneAdicional,
                Email,
                Instagram,
                ContatoEmergenciaNome,
                ContatoEmergenciaCelular,
                Cep,
                Logradouro,
                Numero,
                Complemento,
                Bairro,
                Cidade,
                Estado);
        }
        catch (ArgumentException exception)
        {
            erroDados = new ValidationResult(
                exception.Message.Split(Environment.NewLine)[0],
                exception.ParamName is null ? null : [exception.ParamName]);
        }

        if (erroDados is not null)
        {
            yield return erroDados;
        }
    }
}

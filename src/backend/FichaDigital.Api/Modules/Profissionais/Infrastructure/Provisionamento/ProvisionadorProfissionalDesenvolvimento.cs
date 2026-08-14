using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;

public sealed class ProvisionadorProfissionalDesenvolvimento(
    UserManager<ProfissionalUsuario> userManager,
    IOptions<ProfissionalDesenvolvimentoOptions> options,
    IHostEnvironment environment,
    ILogger<ProvisionadorProfissionalDesenvolvimento> logger)
{
    public async Task ProvisionarAsync()
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        var configuracao = options.Value;
        var nenhumValorConfigurado =
            string.IsNullOrWhiteSpace(configuracao.NomeCompleto) &&
            string.IsNullOrWhiteSpace(configuracao.Email) &&
            string.IsNullOrWhiteSpace(configuracao.Senha);

        if (nenhumValorConfigurado)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(configuracao.NomeCompleto) ||
            string.IsNullOrWhiteSpace(configuracao.Email) ||
            string.IsNullOrWhiteSpace(configuracao.Senha))
        {
            throw new InvalidOperationException(
                "Configure nome, e-mail e senha do profissional de desenvolvimento.");
        }

        var email = configuracao.Email.Trim();
        var profissionalExistente = await userManager.FindByEmailAsync(email);

        if (profissionalExistente is not null)
        {
            return;
        }

        var profissional = new ProfissionalUsuario(
            configuracao.NomeCompleto,
            email)
        {
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(
            profissional,
            configuracao.Senha);

        if (!resultado.Succeeded)
        {
            var erros = string.Join(
                " ",
                resultado.Errors.Select(erro => erro.Description));

            throw new InvalidOperationException(
                $"Não foi possível criar o profissional de desenvolvimento. {erros}");
        }

        logger.LogInformation(
            "Profissional de desenvolvimento {ProfissionalId} criado.",
            profissional.Id);
    }
}

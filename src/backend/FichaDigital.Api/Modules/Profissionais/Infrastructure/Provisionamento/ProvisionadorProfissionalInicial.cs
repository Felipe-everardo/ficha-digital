using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FichaDigital.Api.Modules.Profissionais.Infrastructure.Provisionamento;

public sealed class ProvisionadorProfissionalInicial(
    UserManager<ProfissionalUsuario> userManager,
    IOptions<ProfissionalInicialOptions> options,
    ILogger<ProvisionadorProfissionalInicial> logger)
{
    public async Task ProvisionarAsync()
    {
        var configuracao = options.Value;

        if (!configuracao.Habilitado)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(configuracao.NomeCompleto) ||
            string.IsNullOrWhiteSpace(configuracao.Email) ||
            string.IsNullOrWhiteSpace(configuracao.Senha))
        {
            throw new InvalidOperationException(
                "Configure nome, e-mail e senha do profissional inicial.");
        }

        var email = configuracao.Email.Trim();
        var profissionalExistente = await userManager.FindByEmailAsync(email);

        if (profissionalExistente is not null)
        {
            if (configuracao.RedefinirSenhaSeExistente)
            {
                await RedefinirSenhaAsync(
                    profissionalExistente,
                    configuracao.Senha);
                return;
            }

            logger.LogInformation(
                "O profissional inicial já existe. O provisionamento não alterou a conta.");
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
                $"Não foi possível criar o profissional inicial. {erros}");
        }

        logger.LogInformation(
            "Profissional inicial {ProfissionalId} criado. Desabilite o provisionamento e remova a senha da configuração.",
            profissional.Id);
    }

    private async Task RedefinirSenhaAsync(
        ProfissionalUsuario profissional,
        string novaSenha)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(
            profissional);
        var redefinicao = await userManager.ResetPasswordAsync(
            profissional,
            token,
            novaSenha);

        ValidarResultado(
            redefinicao,
            "Não foi possível redefinir a senha do profissional inicial.");

        var desbloqueio = await userManager.SetLockoutEndDateAsync(
            profissional,
            null);

        ValidarResultado(
            desbloqueio,
            "A senha foi redefinida, mas não foi possível desbloquear a conta.");

        var reinicioTentativas = await userManager.ResetAccessFailedCountAsync(
            profissional);

        ValidarResultado(
            reinicioTentativas,
            "A senha foi redefinida, mas não foi possível zerar as tentativas de acesso.");

        logger.LogWarning(
            "A senha do profissional inicial {ProfissionalId} foi redefinida. Desative a redefinição e remova a senha da configuração imediatamente.",
            profissional.Id);
    }

    private static void ValidarResultado(
        IdentityResult resultado,
        string mensagem)
    {
        if (resultado.Succeeded)
        {
            return;
        }

        var erros = string.Join(
            " ",
            resultado.Errors.Select(erro => erro.Description));

        throw new InvalidOperationException($"{mensagem} {erros}");
    }

}

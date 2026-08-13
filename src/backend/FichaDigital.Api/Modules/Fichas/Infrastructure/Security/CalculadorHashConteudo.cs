using System.Security.Cryptography;
using System.Text;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure.Security;

public sealed class CalculadorHashConteudo
{
    public string Calcular(string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            throw new ArgumentException(
                "O conteúdo é obrigatório.",
                nameof(conteudo));
        }

        var bytesConteudo = Encoding.UTF8.GetBytes(conteudo.Trim());
        var bytesHash = SHA256.HashData(bytesConteudo);

        return Convert.ToHexString(bytesHash)
            .ToLowerInvariant();
    }
}

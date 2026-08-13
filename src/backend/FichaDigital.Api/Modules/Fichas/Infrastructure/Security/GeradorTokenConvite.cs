using System.Security.Cryptography;
using System.Text;

namespace FichaDigital.Api.Modules.Fichas.Infrastructure.Security;

public sealed class GeradorTokenConvite
{
    private const int QuantidadeBytesToken = 32;

    public TokenConviteGerado Gerar()
    {
        var bytesToken = RandomNumberGenerator.GetBytes(
            QuantidadeBytesToken);

        var tokenOriginal = ConverterParaBase64Url(bytesToken);
        var tokenHash = CalcularHash(tokenOriginal);

        return new TokenConviteGerado(
            tokenOriginal,
            tokenHash);
    }

    public string CalcularHash(string tokenOriginal)
    {
        if (string.IsNullOrWhiteSpace(tokenOriginal))
        {
            throw new ArgumentException(
                "O token original é obrigatório.",
                nameof(tokenOriginal));
        }

        var bytesToken = Encoding.UTF8.GetBytes(tokenOriginal);
        var bytesHash = SHA256.HashData(bytesToken);

        return Convert.ToHexString(bytesHash)
            .ToLowerInvariant();
    }

    private static string ConverterParaBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

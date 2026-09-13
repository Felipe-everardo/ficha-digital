namespace FichaDigital.Api.Modules.Fichas.Domain;

public static class AssinaturaDesenhada
{
    public const int TamanhoMaximo = 350_000;

    private const string PrefixoPng = "data:image/png;base64,";

    public static string ValidarENormalizar(
        string? conteudo,
        string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            throw new ArgumentException(
                "A assinatura desenhada é obrigatória.",
                nomeParametro);
        }

        var normalizado = conteudo.Trim();

        if (normalizado.Length > TamanhoMaximo)
        {
            throw new ArgumentException(
                "A assinatura desenhada excede o tamanho permitido.",
                nomeParametro);
        }

        if (!normalizado.StartsWith(
                PrefixoPng,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "A assinatura desenhada deve ser uma imagem PNG válida.",
                nomeParametro);
        }

        var base64 = normalizado[PrefixoPng.Length..];
        if (base64.Length < 50 || !EhBase64Valido(base64))
        {
            throw new ArgumentException(
                "A assinatura desenhada deve ser uma imagem PNG válida.",
                nomeParametro);
        }

        return normalizado;
    }

    private static bool EhBase64Valido(string conteudo)
    {
        var tamanhoMaximoBytes = (conteudo.Length / 4 * 3) + 3;
        var buffer = new byte[tamanhoMaximoBytes];

        return Convert.TryFromBase64String(
            conteudo,
            buffer,
            out var bytesEscritos) &&
            bytesEscritos >= 8 &&
            buffer[0] == 0x89 &&
            buffer[1] == 0x50 &&
            buffer[2] == 0x4E &&
            buffer[3] == 0x47 &&
            buffer[4] == 0x0D &&
            buffer[5] == 0x0A &&
            buffer[6] == 0x1A &&
            buffer[7] == 0x0A;
    }
}

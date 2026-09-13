using System.Globalization;
using System.Text;

namespace FichaDigital.Api.Modules.Fichas.Domain;

public static class ComparadorNomes
{
    public static bool Correspondem(string informado, string esperado)
    {
        return string.Equals(
            Normalizar(informado),
            Normalizar(esperado),
            StringComparison.Ordinal);
    }

    private static string Normalizar(string nome)
    {
        var semAcentos = nome
            .Trim()
            .Normalize(NormalizationForm.FormD)
            .Where(caractere =>
                CharUnicodeInfo.GetUnicodeCategory(caractere) !=
                UnicodeCategory.NonSpacingMark)
            .ToArray();

        return string.Join(
                ' ',
                new string(semAcentos).Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }
}

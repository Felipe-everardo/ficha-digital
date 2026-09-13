namespace FichaDigital.Api.Modules.Fichas.Domain;

internal static class ValidacoesRegistroProcedimento
{
    public static string TextoObrigatorio(
        string? valor,
        int tamanhoMaximo,
        string mensagemObrigatorio,
        string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException(mensagemObrigatorio, nomeParametro);
        }

        var normalizado = valor.Trim();
        if (normalizado.Length > tamanhoMaximo)
        {
            throw new ArgumentException(
                $"O campo deve ter no máximo {tamanhoMaximo} caracteres.",
                nomeParametro);
        }

        return normalizado;
    }

    public static string? TextoOpcional(
        string? valor,
        int tamanhoMaximo,
        string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        return TextoObrigatorio(
            valor,
            tamanhoMaximo,
            "O campo é obrigatório.",
            nomeParametro);
    }

    public static void ValidarValores(decimal valorTotal, decimal valorSinal)
    {
        if (valorTotal < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valorTotal),
                "O valor total não pode ser negativo.");
        }

        if (valorSinal < 0 || valorSinal > valorTotal)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valorSinal),
                "O sinal deve estar entre zero e o valor total.");
        }
    }

    public static void ValidarIdentificacao(
        Guid fichaId,
        Guid profissionalId,
        FormaPagamento formaPagamento,
        DateTimeOffset registradoEmUtc)
    {
        if (fichaId == Guid.Empty || profissionalId == Guid.Empty)
        {
            throw new ArgumentException(
                "A ficha e o profissional são obrigatórios.");
        }

        if (!Enum.IsDefined(formaPagamento))
        {
            throw new ArgumentException(
                "A forma de pagamento é inválida.",
                nameof(formaPagamento));
        }

        if (registradoEmUtc == default)
        {
            throw new ArgumentException(
                "A data do registro é obrigatória.",
                nameof(registradoEmUtc));
        }
    }

    public static string ValidarHash(string hash, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Trim().Length != 64)
        {
            throw new ArgumentException(
                "O código de integridade deve possuir 64 caracteres.",
                nomeParametro);
        }

        return hash.Trim().ToLowerInvariant();
    }
}

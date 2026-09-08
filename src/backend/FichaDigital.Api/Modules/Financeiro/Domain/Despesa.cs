namespace FichaDigital.Api.Modules.Financeiro.Domain;

public sealed class Despesa
{
    private const int TamanhoMaximoDescricao = 200;

    private Despesa()
    {
    }

    public Despesa(
        DateOnly data,
        CategoriaDespesa categoria,
        string descricao,
        decimal valor,
        Guid profissionalId,
        string profissionalNome,
        DateTimeOffset registradaEmUtc)
    {
        if (profissionalId == Guid.Empty)
        {
            throw new ArgumentException(
                "O profissional é obrigatório.",
                nameof(profissionalId));
        }

        if (string.IsNullOrWhiteSpace(profissionalNome))
        {
            throw new ArgumentException(
                "O nome do profissional é obrigatório.",
                nameof(profissionalNome));
        }

        if (registradaEmUtc == default)
        {
            throw new ArgumentException(
                "A data de registro é obrigatória.",
                nameof(registradaEmUtc));
        }

        Id = Guid.NewGuid();
        ProfissionalId = profissionalId;
        ProfissionalNome = profissionalNome.Trim();
        RegistradaEmUtc = registradaEmUtc;
        Atualizar(data, categoria, descricao, valor, registradaEmUtc);
    }

    public Guid Id { get; private set; }

    public DateOnly Data { get; private set; }

    public CategoriaDespesa Categoria { get; private set; }

    public string Descricao { get; private set; } = string.Empty;

    public decimal Valor { get; private set; }

    public Guid ProfissionalId { get; private set; }

    public string ProfissionalNome { get; private set; } = string.Empty;

    public DateTimeOffset RegistradaEmUtc { get; private set; }

    public DateTimeOffset AtualizadaEmUtc { get; private set; }

    public void Atualizar(
        DateOnly data,
        CategoriaDespesa categoria,
        string descricao,
        decimal valor,
        DateTimeOffset atualizadaEmUtc)
    {
        if (data == default)
        {
            throw new ArgumentException(
                "A data da despesa é obrigatória.",
                nameof(data));
        }

        if (!Enum.IsDefined(categoria))
        {
            throw new ArgumentException(
                "A categoria da despesa é inválida.",
                nameof(categoria));
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException(
                "A descrição da despesa é obrigatória.",
                nameof(descricao));
        }

        if (descricao.Trim().Length > TamanhoMaximoDescricao)
        {
            throw new ArgumentException(
                "A descrição deve ter no máximo 200 caracteres.",
                nameof(descricao));
        }

        if (valor <= 0 || valor > 999_999.99m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valor),
                "O valor deve estar entre 0,01 e 999.999,99.");
        }

        if (decimal.Round(valor, 2) != valor)
        {
            throw new ArgumentException(
                "O valor deve ter no máximo duas casas decimais.",
                nameof(valor));
        }

        if (atualizadaEmUtc == default)
        {
            throw new ArgumentException(
                "A data de atualização é obrigatória.",
                nameof(atualizadaEmUtc));
        }

        Data = data;
        Categoria = categoria;
        Descricao = descricao.Trim();
        Valor = valor;
        AtualizadaEmUtc = atualizadaEmUtc;
    }
}

namespace FichaDigital.Api.Modules.Atendimentos.Domain;

public sealed class Atendimento
{
    private Atendimento()
    {
    }

    public Atendimento(
        Guid fichaId,
        DateOnly dataRealizacao,
        decimal valorCobrado,
        decimal desconto,
        FormaPagamento formaPagamento,
        DateTimeOffset registradoEmUtc)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException(
                "A ficha é obrigatória.",
                nameof(fichaId));
        }

        if (registradoEmUtc == default)
        {
            throw new ArgumentException(
                "A data de registro é obrigatória.",
                nameof(registradoEmUtc));
        }

        Id = Guid.NewGuid();
        FichaId = fichaId;
        RegistradoEmUtc = registradoEmUtc;
        Atualizar(
            dataRealizacao,
            valorCobrado,
            desconto,
            formaPagamento,
            registradoEmUtc);
    }

    public Guid Id { get; private set; }

    public Guid FichaId { get; private set; }

    public DateOnly DataRealizacao { get; private set; }

    public decimal ValorCobrado { get; private set; }

    public decimal Desconto { get; private set; }

    public decimal ValorFinal { get; private set; }

    public FormaPagamento FormaPagamento { get; private set; }

    public SituacaoPagamento SituacaoPagamento { get; private set; }

    public DateTimeOffset RegistradoEmUtc { get; private set; }

    public DateTimeOffset AtualizadoEmUtc { get; private set; }

    public void Atualizar(
        DateOnly dataRealizacao,
        decimal valorCobrado,
        decimal desconto,
        FormaPagamento formaPagamento,
        DateTimeOffset atualizadoEmUtc)
    {
        if (dataRealizacao == default)
        {
            throw new ArgumentException(
                "A data do procedimento é obrigatória.",
                nameof(dataRealizacao));
        }

        ValidarValorMonetario(valorCobrado, nameof(valorCobrado));
        ValidarValorMonetario(desconto, nameof(desconto));

        if (desconto > valorCobrado)
        {
            throw new ArgumentException(
                "O desconto não pode ser maior que o valor cobrado.",
                nameof(desconto));
        }

        if (!Enum.IsDefined(formaPagamento))
        {
            throw new ArgumentException(
                "A forma de pagamento é inválida.",
                nameof(formaPagamento));
        }

        if (atualizadoEmUtc == default)
        {
            throw new ArgumentException(
                "A data de atualização é obrigatória.",
                nameof(atualizadoEmUtc));
        }

        DataRealizacao = dataRealizacao;
        ValorCobrado = valorCobrado;
        Desconto = desconto;
        ValorFinal = valorCobrado - desconto;
        FormaPagamento = formaPagamento;
        SituacaoPagamento = SituacaoPagamento.Pago;
        AtualizadoEmUtc = atualizadoEmUtc;
    }

    private static void ValidarValorMonetario(decimal valor, string parametro)
    {
        if (valor < 0 || valor > 999_999.99m)
        {
            throw new ArgumentOutOfRangeException(
                parametro,
                "O valor deve estar entre zero e 999.999,99.");
        }

        if (decimal.Round(valor, 2) != valor)
        {
            throw new ArgumentException(
                "O valor deve ter no máximo duas casas decimais.",
                parametro);
        }
    }
}

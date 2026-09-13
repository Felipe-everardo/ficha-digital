namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class RegistroPiercing
{
    private RegistroPiercing()
    {
    }

    public RegistroPiercing(
        Guid fichaId,
        Guid profissionalId,
        string profissionalNome,
        string joiaUtilizada,
        string agulhaUtilizada,
        string localPerfuracao,
        string? observacoes,
        decimal valorTotal,
        decimal valorSinal,
        FormaPagamento formaPagamento,
        string nomeProfissionalAssinante,
        string assinaturaDesenhada,
        string evidenciaJson,
        string evidenciaHash,
        DateTimeOffset registradoEmUtc)
    {
        ValidacoesRegistroProcedimento.ValidarIdentificacao(
            fichaId,
            profissionalId,
            formaPagamento,
            registradoEmUtc);
        ValidacoesRegistroProcedimento.ValidarValores(valorTotal, valorSinal);

        FichaId = fichaId;
        ProfissionalId = profissionalId;
        ProfissionalNome = ValidacoesRegistroProcedimento.TextoObrigatorio(
            profissionalNome,
            150,
            "O nome do profissional é obrigatório.",
            nameof(profissionalNome));
        JoiaUtilizada = ValidacoesRegistroProcedimento.TextoObrigatorio(
            joiaUtilizada,
            500,
            "A joia utilizada é obrigatória.",
            nameof(joiaUtilizada));
        AgulhaUtilizada = ValidacoesRegistroProcedimento.TextoObrigatorio(
            agulhaUtilizada,
            500,
            "A agulha utilizada é obrigatória.",
            nameof(agulhaUtilizada));
        LocalPerfuracao = ValidacoesRegistroProcedimento.TextoObrigatorio(
            localPerfuracao,
            300,
            "O local da perfuração é obrigatório.",
            nameof(localPerfuracao));
        Observacoes = ValidacoesRegistroProcedimento.TextoOpcional(
            observacoes,
            2000,
            nameof(observacoes));
        ValorTotal = valorTotal;
        ValorSinal = valorSinal;
        FormaPagamento = formaPagamento;
        NomeProfissionalAssinante =
            ValidacoesRegistroProcedimento.TextoObrigatorio(
                nomeProfissionalAssinante,
                150,
                "O nome do profissional assinante é obrigatório.",
                nameof(nomeProfissionalAssinante));
        AssinaturaDesenhada =
            global::FichaDigital.Api.Modules.Fichas.Domain.AssinaturaDesenhada
                .ValidarENormalizar(
                    assinaturaDesenhada,
                    nameof(assinaturaDesenhada));
        EvidenciaJson = ValidacoesRegistroProcedimento.TextoObrigatorio(
            evidenciaJson,
            int.MaxValue,
            "A evidência do registro é obrigatória.",
            nameof(evidenciaJson));
        EvidenciaHash = ValidacoesRegistroProcedimento.ValidarHash(
            evidenciaHash,
            nameof(evidenciaHash));
        RegistradoEmUtc = registradoEmUtc;
    }

    public Guid FichaId { get; private set; }
    public Guid ProfissionalId { get; private set; }
    public string ProfissionalNome { get; private set; } = string.Empty;
    public string JoiaUtilizada { get; private set; } = string.Empty;
    public string AgulhaUtilizada { get; private set; } = string.Empty;
    public string LocalPerfuracao { get; private set; } = string.Empty;
    public string? Observacoes { get; private set; }
    public decimal ValorTotal { get; private set; }
    public decimal ValorSinal { get; private set; }
    public FormaPagamento FormaPagamento { get; private set; }
    public string NomeProfissionalAssinante { get; private set; } = string.Empty;
    public string AssinaturaDesenhada { get; private set; } = string.Empty;
    public string EvidenciaJson { get; private set; } = string.Empty;
    public string EvidenciaHash { get; private set; } = string.Empty;
    public DateTimeOffset RegistradoEmUtc { get; private set; }

    public decimal ValorRestante => ValorTotal - ValorSinal;
}

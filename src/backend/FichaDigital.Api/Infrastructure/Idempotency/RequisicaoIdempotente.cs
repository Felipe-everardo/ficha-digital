namespace FichaDigital.Api.Infrastructure.Idempotency;

public sealed class RequisicaoIdempotente
{
    private RequisicaoIdempotente()
    {
    }

    public RequisicaoIdempotente(
        string escopo,
        string chave,
        string fingerprint,
        string metodo,
        string caminho,
        DateTimeOffset criadaEmUtc)
    {
        Id = Guid.NewGuid();
        Escopo = escopo;
        Chave = chave;
        Fingerprint = fingerprint;
        Metodo = metodo;
        Caminho = caminho;
        CriadaEmUtc = criadaEmUtc;
        ExpiraEmUtc = criadaEmUtc.AddHours(24);
    }

    public Guid Id { get; private set; }
    public string Escopo { get; private set; } = string.Empty;
    public string Chave { get; private set; } = string.Empty;
    public string Fingerprint { get; private set; } = string.Empty;
    public string Metodo { get; private set; } = string.Empty;
    public string Caminho { get; private set; } = string.Empty;
    public int? StatusCode { get; private set; }
    public string? TipoConteudo { get; private set; }
    public string? CorpoRespostaProtegido { get; private set; }
    public string? Localizacao { get; private set; }
    public DateTimeOffset CriadaEmUtc { get; private set; }
    public DateTimeOffset ExpiraEmUtc { get; private set; }
    public DateTimeOffset? ConcluidaEmUtc { get; private set; }

    public bool Concluida => ConcluidaEmUtc is not null;

    public void Concluir(
        int statusCode,
        string? tipoConteudo,
        string corpoRespostaProtegido,
        string? localizacao,
        DateTimeOffset concluidaEmUtc)
    {
        StatusCode = statusCode;
        TipoConteudo = tipoConteudo;
        CorpoRespostaProtegido = corpoRespostaProtegido;
        Localizacao = localizacao;
        ConcluidaEmUtc = concluidaEmUtc;
    }
}

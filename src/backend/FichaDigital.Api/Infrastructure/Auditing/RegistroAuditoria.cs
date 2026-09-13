namespace FichaDigital.Api.Infrastructure.Auditing;

public sealed class RegistroAuditoria
{
    private RegistroAuditoria()
    {
    }

    public RegistroAuditoria(
        Guid profissionalId,
        string acao,
        string recurso,
        Guid? recursoId,
        string correlacaoId,
        DateTimeOffset ocorreuEmUtc)
        : this(
            profissionalId,
            "Profissional",
            acao,
            recurso,
            recursoId,
            correlacaoId,
            ocorreuEmUtc)
    {
    }

    private RegistroAuditoria(
        Guid? profissionalId,
        string origem,
        string acao,
        string recurso,
        Guid? recursoId,
        string correlacaoId,
        DateTimeOffset ocorreuEmUtc)
    {
        Id = Guid.NewGuid();
        ProfissionalId = profissionalId;
        Origem = origem;
        Acao = acao;
        Recurso = recurso;
        RecursoId = recursoId;
        CorrelacaoId = correlacaoId;
        OcorreuEmUtc = ocorreuEmUtc;
    }

    public Guid Id { get; private set; }

    public Guid? ProfissionalId { get; private set; }

    public string Origem { get; private set; } = string.Empty;

    public string Acao { get; private set; } = string.Empty;

    public string Recurso { get; private set; } = string.Empty;

    public Guid? RecursoId { get; private set; }

    public string CorrelacaoId { get; private set; } = string.Empty;

    public DateTimeOffset OcorreuEmUtc { get; private set; }

    public static RegistroAuditoria CriarParaCliente(
        string acao,
        Guid fichaId,
        string correlacaoId,
        DateTimeOffset ocorreuEmUtc)
    {
        return new RegistroAuditoria(
            null,
            "Cliente",
            acao,
            "Ficha",
            fichaId,
            correlacaoId,
            ocorreuEmUtc);
    }
}

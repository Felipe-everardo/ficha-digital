namespace FichaDigital.Api.Modules.Fichas.Domain;

public sealed class AceiteTermoConsentimento
{
    private const int TamanhoMaximoNomeAssinante = 150;
    private const int TamanhoMaximoEnderecoIp = 64;
    private const int TamanhoMaximoAgenteUsuario = 512;

    private AceiteTermoConsentimento()
    {
    }

    public AceiteTermoConsentimento(
        Guid fichaId,
        int versaoTermo,
        string conteudoTermo,
        string conteudoHash,
        string nomeAssinante,
        DateTimeOffset aceitoEmUtc)
        : this(
            fichaId,
            conviteId: null,
            versaoTermo,
            conteudoTermo,
            conteudoHash,
            nomeAssinante,
            confirmouLeituraEAutorizacao: true,
            versaoEvidencia: 1,
            evidenciaJson: "{}",
            evidenciaHash: conteudoHash,
            enderecoIp: null,
            agenteUsuario: null,
            aceitoEmUtc,
            assinaturaDesenhada: null)
    {
    }

    public AceiteTermoConsentimento(
        Guid fichaId,
        Guid? conviteId,
        int versaoTermo,
        string conteudoTermo,
        string conteudoHash,
        string nomeAssinante,
        bool confirmouLeituraEAutorizacao,
        int versaoEvidencia,
        string evidenciaJson,
        string evidenciaHash,
        string? enderecoIp,
        string? agenteUsuario,
        DateTimeOffset aceitoEmUtc,
        string? assinaturaDesenhada = null)
    {
        if (fichaId == Guid.Empty)
        {
            throw new ArgumentException(
                "A ficha é obrigatória.",
                nameof(fichaId));
        }

        if (versaoTermo <= 0)
        {
            throw new ArgumentException(
                "A versão do termo deve ser maior que zero.",
                nameof(versaoTermo));
        }

        if (conviteId == Guid.Empty)
        {
            throw new ArgumentException(
                "O convite é inválido.",
                nameof(conviteId));
        }

        if (string.IsNullOrWhiteSpace(conteudoTermo))
        {
            throw new ArgumentException(
                "O conteúdo do termo é obrigatório.",
                nameof(conteudoTermo));
        }

        if (string.IsNullOrWhiteSpace(conteudoHash) ||
            conteudoHash.Trim().Length != 64)
        {
            throw new ArgumentException(
                "O hash do conteúdo deve possuir 64 caracteres.",
                nameof(conteudoHash));
        }

        if (string.IsNullOrWhiteSpace(nomeAssinante))
        {
            throw new ArgumentException(
                "O nome do assinante é obrigatório.",
                nameof(nomeAssinante));
        }

        if (nomeAssinante.Trim().Length > TamanhoMaximoNomeAssinante)
        {
            throw new ArgumentException(
                "O nome do assinante deve ter no máximo 150 caracteres.",
                nameof(nomeAssinante));
        }

        if (!confirmouLeituraEAutorizacao)
        {
            throw new ArgumentException(
                "A leitura e a autorização do procedimento precisam ser confirmadas.",
                nameof(confirmouLeituraEAutorizacao));
        }

        if (versaoEvidencia <= 0)
        {
            throw new ArgumentException(
                "A versão da evidência deve ser maior que zero.",
                nameof(versaoEvidencia));
        }

        if (string.IsNullOrWhiteSpace(evidenciaJson))
        {
            throw new ArgumentException(
                "O conteúdo da evidência é obrigatório.",
                nameof(evidenciaJson));
        }

        if (string.IsNullOrWhiteSpace(evidenciaHash) ||
            evidenciaHash.Trim().Length != 64)
        {
            throw new ArgumentException(
                "O hash da evidência deve possuir 64 caracteres.",
                nameof(evidenciaHash));
        }

        ValidarTamanhoOpcional(
            enderecoIp,
            TamanhoMaximoEnderecoIp,
            "O endereço IP deve ter no máximo 64 caracteres.",
            nameof(enderecoIp));
        ValidarTamanhoOpcional(
            agenteUsuario,
            TamanhoMaximoAgenteUsuario,
            "A identificação do navegador deve ter no máximo 512 caracteres.",
            nameof(agenteUsuario));

        if (aceitoEmUtc == default)
        {
            throw new ArgumentException(
                "A data do aceite é obrigatória.",
                nameof(aceitoEmUtc));
        }

        Id = Guid.NewGuid();
        FichaId = fichaId;
        ConviteId = conviteId;
        VersaoTermo = versaoTermo;
        ConteudoTermo = conteudoTermo.Trim();
        ConteudoHash = conteudoHash.Trim().ToLowerInvariant();
        NomeAssinante = nomeAssinante.Trim();
        ConfirmouLeituraEAutorizacao = confirmouLeituraEAutorizacao;
        VersaoEvidencia = versaoEvidencia;
        EvidenciaJson = evidenciaJson.Trim();
        EvidenciaHash = evidenciaHash.Trim().ToLowerInvariant();
        EnderecoIp = NormalizarOpcional(enderecoIp);
        AgenteUsuario = NormalizarOpcional(agenteUsuario);
        AceitoEmUtc = aceitoEmUtc;
        AssinaturaDesenhada = assinaturaDesenhada is null
            ? null
            : global::FichaDigital.Api.Modules.Fichas.Domain.AssinaturaDesenhada
                .ValidarENormalizar(
                assinaturaDesenhada,
                nameof(assinaturaDesenhada));
    }

    public Guid Id { get; private set; }

    public Guid FichaId { get; private set; }

    public Guid? ConviteId { get; private set; }

    public int VersaoTermo { get; private set; }

    public string ConteudoTermo { get; private set; } = string.Empty;

    public string ConteudoHash { get; private set; } = string.Empty;

    public string NomeAssinante { get; private set; } = string.Empty;

    public bool ConfirmouLeituraEAutorizacao { get; private set; }

    public int VersaoEvidencia { get; private set; }

    public string EvidenciaJson { get; private set; } = string.Empty;

    public string EvidenciaHash { get; private set; } = string.Empty;

    public string? EnderecoIp { get; private set; }

    public string? AgenteUsuario { get; private set; }

    public DateTimeOffset AceitoEmUtc { get; private set; }

    public string? AssinaturaDesenhada { get; private set; }

    private static void ValidarTamanhoOpcional(
        string? valor,
        int tamanhoMaximo,
        string mensagem,
        string nomeParametro)
    {
        if (valor?.Trim().Length > tamanhoMaximo)
        {
            throw new ArgumentException(mensagem, nomeParametro);
        }
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}

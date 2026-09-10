namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record AceitarTermoConsentimentoCommand(
    string TokenOriginal,
    int VersaoTermo,
    string ConteudoHash,
    string NomeAssinante,
    bool ConfirmouMaioridade,
    bool ConfirmouDadosPessoais,
    bool ConfirmouQuestionarioSaude,
    string? EnderecoIp,
    string? AgenteUsuario);

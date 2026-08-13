namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record AceitarTermoConsentimentoCommand(
    string TokenOriginal,
    int VersaoTermo,
    string ConteudoHash,
    string NomeAssinante);

namespace FichaDigital.Api.Modules.Fichas.Api;

public sealed record TermoConsentimentoResponse(
    int Versao,
    string Conteudo,
    string ConteudoHash);

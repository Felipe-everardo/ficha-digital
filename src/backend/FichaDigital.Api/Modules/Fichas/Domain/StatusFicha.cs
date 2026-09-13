namespace FichaDigital.Api.Modules.Fichas.Domain;

public enum StatusFicha
{
    Rascunho = 1,
    ConviteEnviado = 2,
    EmPreenchimento = 3,
    Concluida = 4,
    Expirada = 5,
    Cancelada = 6,
    AnamnesePreenchida = 7,
    // Mantido apenas para leitura de fichas criadas pelo fluxo anterior.
    AguardandoConsentimento = 8,
    AutorizadaParaProcedimento = 9,
    RevisadaPeloProfissional = 10
}

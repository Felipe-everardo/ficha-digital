namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ModeloFichaSelecionado(
    int VersaoModelo,
    int VersaoQuestionario,
    int VersaoTermo,
    string CnpjApresentado,
    string ConteudoTermo);

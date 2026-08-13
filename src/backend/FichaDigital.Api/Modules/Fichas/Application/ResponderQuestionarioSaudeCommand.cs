namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResponderQuestionarioSaudeCommand(
    string TokenOriginal,
    bool TemDiabetes,
    string? TipoDiabetes,
    bool PossuiPressaoAlta,
    bool TemAlergia,
    string? DescricaoAlergia,
    bool PossuiCondicaoCardiaca,
    bool TemEpilepsia,
    bool TemHemofilia,
    bool UsaMarcaPasso,
    bool EstaGravidaOuAmamentando);

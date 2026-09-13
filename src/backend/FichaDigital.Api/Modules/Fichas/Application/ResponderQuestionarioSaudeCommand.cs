namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResponderQuestionarioSaudeCommand(
    string TokenOriginal,
    bool TemDiabetes,
    string? TipoDiabetes,
    bool TeveAnemia,
    string? DescricaoAnemia,
    bool TeveHepatite,
    string? TipoHepatite,
    bool PossuiPressaoAlta,
    bool TemAlergia,
    string? DescricaoAlergia,
    bool PossuiCondicaoCardiaca,
    bool TemEpilepsia,
    bool TemHemofilia,
    bool PossuiDoencaTransmissivel,
    string? DescricaoDoencaTransmissivel,
    bool UsaMarcaPasso,
    bool Fuma,
    bool ConsumiuBebidaAlcoolicaUltimas24Horas,
    bool UsaMedicacao,
    string? DescricaoMedicacao,
    bool EstaGravidaOuAmamentando);

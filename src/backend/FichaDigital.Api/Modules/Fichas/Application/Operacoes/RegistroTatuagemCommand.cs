namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record RegistroTatuagemCommand(
    string ArteEfetivamenteTatuada,
    string MaterialUtilizado,
    string LocalTatuagem,
    string? Observacoes);

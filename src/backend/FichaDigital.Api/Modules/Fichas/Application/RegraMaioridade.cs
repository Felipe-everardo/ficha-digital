namespace FichaDigital.Api.Modules.Fichas.Application;

public static class RegraMaioridade
{
    private const int IdadeMinima = 18;

    public static bool EhMaiorDeIdade(
        DateOnly dataNascimento,
        DateTimeOffset instanteUtc)
    {
        var dataNoRio = HorarioEstudio.ObterDataLocal(instanteUtc);

        return dataNascimento <= dataNoRio.AddYears(-IdadeMinima);
    }
}

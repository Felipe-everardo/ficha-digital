namespace FichaDigital.Api.Modules.Fichas.Application;

public static class RegraMaioridade
{
    private const int IdadeMinima = 18;

    private static readonly TimeZoneInfo FusoHorarioRio =
        ObterFusoHorarioRio();

    public static bool EhMaiorDeIdade(
        DateOnly dataNascimento,
        DateTimeOffset instanteUtc)
    {
        var instanteNoRio = TimeZoneInfo.ConvertTime(
            instanteUtc,
            FusoHorarioRio);
        var dataNoRio = DateOnly.FromDateTime(instanteNoRio.DateTime);

        return dataNascimento <= dataNoRio.AddYears(-IdadeMinima);
    }

    private static TimeZoneInfo ObterFusoHorarioRio()
    {
        foreach (var identificador in new[]
                 {
                     "America/Sao_Paulo",
                     "E. South America Standard Time"
                 })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(identificador);
            }
            catch (TimeZoneNotFoundException)
            {
            }
        }

        throw new InvalidOperationException(
            "Não foi possível localizar o fuso horário do Rio de Janeiro.");
    }
}

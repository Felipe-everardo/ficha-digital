namespace FichaDigital.Api.Modules.Fichas.Application;

public static class HorarioEstudio
{
    private static readonly TimeZoneInfo FusoHorario = ObterFusoHorario();

    public static DateOnly ObterDataLocal(DateTimeOffset instanteUtc)
    {
        var instanteLocal = TimeZoneInfo.ConvertTime(instanteUtc, FusoHorario);
        return DateOnly.FromDateTime(instanteLocal.DateTime);
    }

    public static DateTimeOffset ObterInicioUtc(DateOnly dataLocal)
    {
        var inicioLocal = DateTime.SpecifyKind(
            dataLocal.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified);
        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(inicioLocal, FusoHorario);
        return new DateTimeOffset(inicioUtc, TimeSpan.Zero);
    }

    private static TimeZoneInfo ObterFusoHorario()
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
            "Não foi possível localizar o fuso horário do estúdio.");
    }
}

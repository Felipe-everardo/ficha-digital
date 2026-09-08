using System.Text.Json.Serialization;

namespace FichaDigital.Api.Modules.Fichas.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoProcedimento
{
    NaoInformado = 0,
    Tatuagem = 1,
    Piercing = 2
}

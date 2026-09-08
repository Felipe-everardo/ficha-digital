using System.Text.Json.Serialization;

namespace FichaDigital.Api.Modules.Atendimentos.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SituacaoPagamento
{
    Pago = 1
}

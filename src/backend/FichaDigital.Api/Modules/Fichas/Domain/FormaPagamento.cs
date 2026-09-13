using System.Text.Json.Serialization;

namespace FichaDigital.Api.Modules.Fichas.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormaPagamento
{
    Pix = 1,
    Dinheiro = 2,
    Cartao = 3
}

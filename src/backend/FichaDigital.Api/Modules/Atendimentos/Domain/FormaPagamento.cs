using System.Text.Json.Serialization;

namespace FichaDigital.Api.Modules.Atendimentos.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormaPagamento
{
    Dinheiro = 1,
    Pix = 2,
    CartaoDebito = 3,
    CartaoCredito = 4,
    Transferencia = 5,
    Outro = 6
}

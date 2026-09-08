using System.Text.Json.Serialization;

namespace FichaDigital.Api.Modules.Financeiro.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CategoriaDespesa
{
    Materiais = 1,
    Aluguel = 2,
    Contas = 3,
    Manutencao = 4,
    Marketing = 5,
    ImpostosETaxas = 6,
    PagamentoProfissional = 7,
    Outro = 8
}

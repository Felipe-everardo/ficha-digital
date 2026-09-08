namespace FichaDigital.Api.Modules.Financeiro.Api;

public sealed record DespesaResponse(
    Guid Id,
    DateOnly Data,
    string Categoria,
    string Descricao,
    decimal Valor,
    Guid ProfissionalId,
    string ProfissionalNome,
    DateTimeOffset RegistradaEmUtc,
    DateTimeOffset AtualizadaEmUtc);

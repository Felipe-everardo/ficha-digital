using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ResultadoAberturaConvite(
    StatusAberturaConvite Resultado,
    Guid? FichaId = null,
    StatusFicha? StatusFicha = null,
    bool QuestionarioRespondido = false,
    bool DadosPessoaisPreenchidos = false,
    string? NomeReferencia = null,
    string? ProfissionalResponsavelNome = null,
    TipoProcedimento? TipoProcedimento = null,
    DadosPessoaisConvite? DadosPessoais = null);

using FichaDigital.Api.Modules.Fichas.Domain;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed record ConcluirProcedimentoCommand(
    Guid FichaId,
    Guid ProfissionalId,
    decimal ValorTotal,
    decimal ValorSinal,
    FormaPagamento FormaPagamento,
    string AssinaturaDesenhada,
    RegistroTatuagemCommand? Tatuagem,
    RegistroPiercingCommand? Piercing);

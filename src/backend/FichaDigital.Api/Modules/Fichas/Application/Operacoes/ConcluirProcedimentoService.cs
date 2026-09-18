using System.Text.Json;
using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class ConcluirProcedimentoService(
    FichaDigitalDbContext dbContext,
    CalculadorHashConteudo calculadorHash,
    TimeProvider timeProvider)
{
    public async Task<ResultadoConclusaoProcedimento> ConcluirAsync(
        ConcluirProcedimentoCommand command,
        CancellationToken cancellationToken)
    {
        var ficha = await dbContext.Fichas.SingleOrDefaultAsync(
            item => item.Id == command.FichaId,
            cancellationToken);

        if (ficha is null)
        {
            return new ResultadoConclusaoProcedimento(
                StatusConclusaoProcedimento.FichaNaoEncontrada);
        }

        if (ficha.ProfissionalResponsavelId != command.ProfissionalId)
        {
            return new ResultadoConclusaoProcedimento(
                StatusConclusaoProcedimento.ProfissionalNaoResponsavel);
        }

        if (ficha.Status != StatusFicha.RevisadaPeloProfissional)
        {
            return new ResultadoConclusaoProcedimento(
                StatusConclusaoProcedimento.FichaIndisponivel);
        }

        var registradoEmUtc = timeProvider.GetUtcNow();
        var profissionalNome = ficha.ProfissionalResponsavelNome;
        var evidenciaJson = JsonSerializer.Serialize(new
        {
            versaoEvidencia = 1,
            ficha.Id,
            tipoProcedimento = ficha.TipoProcedimento.ToString(),
            command.ProfissionalId,
            profissionalNome,
            command.ValorTotal,
            command.ValorSinal,
            formaPagamento = command.FormaPagamento.ToString(),
            command.AssinaturaDesenhada,
            command.Tatuagem,
            command.Piercing,
            registradoEmUtc
        });
        var evidenciaHash = calculadorHash.Calcular(evidenciaJson);

        try
        {
            switch (ficha.TipoProcedimento)
            {
                case TipoProcedimento.Tatuagem
                    when command.Tatuagem is not null &&
                         command.Piercing is null:
                    dbContext.RegistrosTatuagem.Add(new RegistroTatuagem(
                        ficha.Id,
                        command.ProfissionalId,
                        profissionalNome,
                        command.Tatuagem.ArteEfetivamenteTatuada,
                        command.Tatuagem.MaterialUtilizado,
                        command.Tatuagem.LocalTatuagem,
                        command.Tatuagem.Observacoes,
                        command.ValorTotal,
                        command.ValorSinal,
                        command.FormaPagamento,
                        profissionalNome,
                        command.AssinaturaDesenhada,
                        evidenciaJson,
                        evidenciaHash,
                        registradoEmUtc));
                    break;

                case TipoProcedimento.Piercing
                    when command.Piercing is not null &&
                         command.Tatuagem is null:
                    dbContext.RegistrosPiercing.Add(new RegistroPiercing(
                        ficha.Id,
                        command.ProfissionalId,
                        profissionalNome,
                        command.Piercing.JoiaUtilizada,
                        command.Piercing.AgulhaUtilizada,
                        command.Piercing.LocalPerfuracao,
                        command.Piercing.Observacoes,
                        command.ValorTotal,
                        command.ValorSinal,
                        command.FormaPagamento,
                        profissionalNome,
                        command.AssinaturaDesenhada,
                        evidenciaJson,
                        evidenciaHash,
                        registradoEmUtc));
                    break;

                default:
                    return new ResultadoConclusaoProcedimento(
                        StatusConclusaoProcedimento.DadosInvalidos);
            }
        }
        catch (ArgumentException)
        {
            return new ResultadoConclusaoProcedimento(
                StatusConclusaoProcedimento.DadosInvalidos);
        }

        ficha.ConcluirProcedimento();
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoConclusaoProcedimento(
            StatusConclusaoProcedimento.Concluido,
            ficha.Id,
            ficha.Status,
            registradoEmUtc,
            evidenciaHash);
    }
}

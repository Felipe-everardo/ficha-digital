using FichaDigital.Api.Infrastructure.Persistence;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Fichas.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Modules.Fichas.Application;

public sealed class RevisarFichaService(
    FichaDigitalDbContext dbContext,
    CalculadorHashConteudo calculadorHash,
    TimeProvider timeProvider)
{
    public async Task<ResultadoRevisaoFicha> RevisarAsync(
        Guid fichaId,
        Guid profissionalId,
        bool dadosDaFichaConferidos,
        CancellationToken cancellationToken)
    {
        var ficha = await dbContext.Fichas.SingleOrDefaultAsync(
            item => item.Id == fichaId,
            cancellationToken);

        if (ficha is null)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.FichaNaoEncontrada);
        }

        if (ficha.ProfissionalResponsavelId != profissionalId)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.ProfissionalNaoResponsavel);
        }

        if (ficha.Status != StatusFicha.AutorizadaParaProcedimento)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.FichaIndisponivel);
        }

        if (!dadosDaFichaConferidos)
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.DadosNaoConferidos);
        }

        var aceite = await dbContext.AceitesTermoConsentimento
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.FichaId == ficha.Id,
                cancellationToken);

        if (aceite is null ||
            aceite.AssinaturaDesenhada is null ||
            !aceite.ConfirmouLeituraEAutorizacao ||
            !string.Equals(
                calculadorHash.Calcular(aceite.EvidenciaJson),
                aceite.EvidenciaHash,
                StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoRevisaoFicha(
                StatusRevisaoFicha.ConsentimentoInvalido);
        }

        var revisao = new RevisaoProfissional(
            ficha.Id,
            profissionalId,
            ficha.ProfissionalResponsavelNome,
            dadosDaFichaConferidos,
            timeProvider.GetUtcNow());

        ficha.ConfirmarRevisaoProfissional();
        dbContext.RevisoesProfissionais.Add(revisao);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoRevisaoFicha(
            StatusRevisaoFicha.Confirmada,
            ficha.Id,
            ficha.Status,
            revisao.RevisadaEmUtc);
    }
}

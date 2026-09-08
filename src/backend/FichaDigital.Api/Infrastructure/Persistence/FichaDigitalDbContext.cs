using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Atendimentos.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using FichaDigital.Api.Modules.Financeiro.Domain;
using FichaDigital.Api.Modules.Profissionais.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Persistence;

public sealed class FichaDigitalDbContext(
    DbContextOptions<FichaDigitalDbContext> options)
    : IdentityDbContext<
        ProfissionalUsuario,
        IdentityRole<Guid>,
        Guid>(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();

    public DbSet<Ficha> Fichas => Set<Ficha>();

    public DbSet<Despesa> Despesas => Set<Despesa>();

    public DbSet<ConviteFicha> ConvitesFicha => Set<ConviteFicha>();

    public DbSet<QuestionarioSaude> QuestionariosSaude => Set<QuestionarioSaude>();

    public DbSet<AceiteTermoConsentimento> AceitesTermoConsentimento =>
        Set<AceiteTermoConsentimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FichaDigitalDbContext).Assembly);
    }
}

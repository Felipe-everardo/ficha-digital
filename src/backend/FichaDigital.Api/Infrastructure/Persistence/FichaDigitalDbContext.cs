using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Persistence;

public sealed class FichaDigitalDbContext(
    DbContextOptions<FichaDigitalDbContext> options)
    : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Ficha> Fichas => Set<Ficha>();

    public DbSet<ConviteFicha> ConvitesFicha => Set<ConviteFicha>();

    public DbSet<QuestionarioSaude> QuestionariosSaude => Set<QuestionarioSaude>();

    public DbSet<AceiteTermoConsentimento> AceitesTermoConsentimento =>
        Set<AceiteTermoConsentimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FichaDigitalDbContext).Assembly);
    }
}

using FichaDigital.Api.Modules.Clientes.Domain;
using Microsoft.EntityFrameworkCore;

namespace FichaDigital.Api.Infrastructure.Persistence;

public sealed class FichaDigitalDbContext(
    DbContextOptions<FichaDigitalDbContext> options)
    : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FichaDigitalDbContext).Assembly);
    }
}

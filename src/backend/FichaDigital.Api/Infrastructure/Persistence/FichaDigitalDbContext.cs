using FichaDigital.Api.Infrastructure.Auditing;
using FichaDigital.Api.Infrastructure.Idempotency;
using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Modules.Fichas.Domain;
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
    public DbSet<RegistroAuditoria> RegistrosAuditoria =>
        Set<RegistroAuditoria>();

    public DbSet<RequisicaoIdempotente> RequisicoesIdempotentes =>
        Set<RequisicaoIdempotente>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Ficha> Fichas => Set<Ficha>();

    public DbSet<DadosPessoaisFicha> DadosPessoaisFichas =>
        Set<DadosPessoaisFicha>();

    public DbSet<ConviteFicha> ConvitesFicha => Set<ConviteFicha>();

    public DbSet<QuestionarioSaude> QuestionariosSaude => Set<QuestionarioSaude>();

    public DbSet<RevisaoProfissional> RevisoesProfissionais =>
        Set<RevisaoProfissional>();

    public DbSet<RegistroTatuagem> RegistrosTatuagem =>
        Set<RegistroTatuagem>();

    public DbSet<RegistroPiercing> RegistrosPiercing =>
        Set<RegistroPiercing>();

    public DbSet<AceiteTermoConsentimento> AceitesTermoConsentimento =>
        Set<AceiteTermoConsentimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FichaDigitalDbContext).Assembly);
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateProfessionalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OcorrenciasProcedimento");

            migrationBuilder.DropColumn(
                name: "DataPrevistaRetorno",
                table: "RegistrosTatuagem");

            migrationBuilder.CreateTable(
                name: "RevisoesProfissionais",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DadosDaFichaConferidos = table.Column<bool>(type: "bit", nullable: false),
                    RevisadaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisoesProfissionais", x => x.FichaId);
                    table.ForeignKey(
                        name: "FK_RevisoesProfissionais_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [RevisoesProfissionais] (
                    [FichaId], [ProfissionalId], [ProfissionalNome],
                    [DadosDaFichaConferidos], [RevisadaEmUtc])
                SELECT
                    [FichaId], [ProfissionalId], [ProfissionalNome],
                    [IdentidadeClienteConferida], [IniciadoEmUtc]
                FROM [IniciosProcedimento];

                UPDATE [Fichas]
                SET [Status] = N'RevisadaPeloProfissional'
                WHERE [Status] IN (N'EmProcedimento', N'Interrompida');
                """);

            migrationBuilder.DropTable(
                name: "IniciosProcedimento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataPrevistaRetorno",
                table: "RegistrosTatuagem",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IniciosProcedimento",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentidadeClienteConferida = table.Column<bool>(type: "bit", nullable: false),
                    IniciadoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IniciosProcedimento", x => x.FichaId);
                    table.ForeignKey(
                        name: "FK_IniciosProcedimento_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [IniciosProcedimento] (
                    [FichaId], [ProfissionalId], [ProfissionalNome],
                    [IdentidadeClienteConferida], [IniciadoEmUtc])
                SELECT
                    [FichaId], [ProfissionalId], [ProfissionalNome],
                    [DadosDaFichaConferidos], [RevisadaEmUtc]
                FROM [RevisoesProfissionais];

                UPDATE [Fichas]
                SET [Status] = N'EmProcedimento'
                WHERE [Status] = N'RevisadaPeloProfissional';
                """);

            migrationBuilder.DropTable(
                name: "RevisoesProfissionais");

            migrationBuilder.CreateTable(
                name: "OcorrenciasProcedimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssinaturaDesenhada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Etapa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EvidenciaHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EvidenciaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MateriaisOuJoiasUtilizados = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NomeProfissionalAssinante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OrientacoesFornecidas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProvidenciasTomadas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RegistradaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OcorrenciasProcedimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OcorrenciasProcedimento_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OcorrenciasProcedimento_FichaId_RegistradaEmUtc",
                table: "OcorrenciasProcedimento",
                columns: new[] { "FichaId", "RegistradaEmUtc" });
        }
    }
}

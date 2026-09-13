using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcedureExecutionRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IniciosProcedimento",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IdentidadeClienteConferida = table.Column<bool>(type: "bit", nullable: false),
                    IniciadoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "OcorrenciasProcedimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Etapa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ProvidenciasTomadas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OrientacoesFornecidas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MateriaisOuJoiasUtilizados = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NomeProfissionalAssinante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AssinaturaDesenhada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenciaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenciaHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "RegistrosPiercing",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    JoiaUtilizada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AgulhaUtilizada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LocalPerfuracao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ValorSinal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NomeProfissionalAssinante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AssinaturaDesenhada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenciaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenciaHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RegistradoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosPiercing", x => x.FichaId);
                    table.ForeignKey(
                        name: "FK_RegistrosPiercing_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosTatuagem",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ArteEfetivamenteTatuada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaterialUtilizado = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LocalTatuagem = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DataPrevistaRetorno = table.Column<DateOnly>(type: "date", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ValorSinal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NomeProfissionalAssinante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AssinaturaDesenhada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenciaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenciaHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RegistradoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosTatuagem", x => x.FichaId);
                    table.ForeignKey(
                        name: "FK_RegistrosTatuagem_Fichas_FichaId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IniciosProcedimento");

            migrationBuilder.DropTable(
                name: "OcorrenciasProcedimento");

            migrationBuilder.DropTable(
                name: "RegistrosPiercing");

            migrationBuilder.DropTable(
                name: "RegistrosTatuagem");
        }
    }
}

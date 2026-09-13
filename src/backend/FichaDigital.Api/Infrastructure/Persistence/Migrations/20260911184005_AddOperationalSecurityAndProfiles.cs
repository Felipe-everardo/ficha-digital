using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalSecurityAndProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CriadoPorProfissionalId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Origem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Acao = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Recurso = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelacaoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OcorreuEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequisicoesIdempotentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Escopo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Chave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Metodo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Caminho = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    TipoConteudo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CorpoRespostaProtegido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Localizacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CriadaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiraEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConcluidaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicoesIdempotentes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CriadoPorProfissionalId",
                table: "Clientes",
                column: "CriadoPorProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_ProfissionalId",
                table: "RegistrosAuditoria",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_Recurso_RecursoId_OcorreuEmUtc",
                table: "RegistrosAuditoria",
                columns: new[] { "Recurso", "RecursoId", "OcorreuEmUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesIdempotentes_Escopo_Chave",
                table: "RequisicoesIdempotentes",
                columns: new[] { "Escopo", "Chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesIdempotentes_ExpiraEmUtc",
                table: "RequisicoesIdempotentes",
                column: "ExpiraEmUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");

            migrationBuilder.DropTable(
                name: "RequisicoesIdempotentes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_CriadoPorProfissionalId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CriadoPorProfissionalId",
                table: "Clientes");
        }
    }
}

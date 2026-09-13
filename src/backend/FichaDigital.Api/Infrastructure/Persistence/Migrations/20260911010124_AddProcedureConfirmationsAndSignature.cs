using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcedureConfirmationsAndSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssinaturaDesenhada",
                table: "AceitesTermoConsentimento",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfirmacoesPiercing",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersaoModelo = table.Column<int>(type: "int", nullable: false),
                    MarcacaoELocalAprovados = table.Column<bool>(type: "bit", nullable: false),
                    JoiaEscolhida = table.Column<bool>(type: "bit", nullable: false),
                    LaudoTecnicoApresentado = table.Column<bool>(type: "bit", nullable: false),
                    OrientacoesRetornoCompreendidas = table.Column<bool>(type: "bit", nullable: false),
                    AutorizouUsoImagem = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmadasEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmacoesPiercing", x => x.FichaId);
                    table.ForeignKey(
                        name: "FK_ConfirmacoesPiercing_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmacoesTatuagem",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersaoModelo = table.Column<int>(type: "int", nullable: false),
                    ArteAprovada = table.Column<bool>(type: "bit", nullable: false),
                    DecalqueOuFreeHandAprovado = table.Column<bool>(type: "bit", nullable: false),
                    LocalAprovado = table.Column<bool>(type: "bit", nullable: false),
                    OrientacoesRetoqueCompreendidas = table.Column<bool>(type: "bit", nullable: false),
                    AutorizouUsoImagem = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmadasEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmacoesTatuagem", x => x.FichaId);
                    table.ForeignKey(
                        name: "FK_ConfirmacoesTatuagem_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmacoesPiercing");

            migrationBuilder.DropTable(
                name: "ConfirmacoesTatuagem");

            migrationBuilder.DropColumn(
                name: "AssinaturaDesenhada",
                table: "AceitesTermoConsentimento");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyConsentFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmacoesPiercing");

            migrationBuilder.DropTable(
                name: "ConfirmacoesTatuagem");

            migrationBuilder.DropColumn(
                name: "ConfirmouDadosPessoais",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "ConfirmouMaioridade",
                table: "AceitesTermoConsentimento");

            migrationBuilder.RenameColumn(
                name: "ConfirmouQuestionarioSaude",
                table: "AceitesTermoConsentimento",
                newName: "ConfirmouLeituraEAutorizacao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConfirmouLeituraEAutorizacao",
                table: "AceitesTermoConsentimento",
                newName: "ConfirmouQuestionarioSaude");

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmouDadosPessoais",
                table: "AceitesTermoConsentimento",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmouMaioridade",
                table: "AceitesTermoConsentimento",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "ConfirmacoesPiercing",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutorizouUsoImagem = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmadasEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    JoiaEscolhida = table.Column<bool>(type: "bit", nullable: false),
                    LaudoTecnicoApresentado = table.Column<bool>(type: "bit", nullable: false),
                    MarcacaoELocalAprovados = table.Column<bool>(type: "bit", nullable: false),
                    OrientacoesRetornoCompreendidas = table.Column<bool>(type: "bit", nullable: false),
                    VersaoModelo = table.Column<int>(type: "int", nullable: false)
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
                    ArteAprovada = table.Column<bool>(type: "bit", nullable: false),
                    AutorizouUsoImagem = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmadasEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DecalqueOuFreeHandAprovado = table.Column<bool>(type: "bit", nullable: false),
                    LocalAprovado = table.Column<bool>(type: "bit", nullable: false),
                    OrientacoesRetoqueCompreendidas = table.Column<bool>(type: "bit", nullable: false),
                    VersaoModelo = table.Column<int>(type: "int", nullable: false)
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
    }
}

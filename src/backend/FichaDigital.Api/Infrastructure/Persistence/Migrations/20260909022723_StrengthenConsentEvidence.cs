using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StrengthenConsentEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgenteUsuario",
                table: "AceitesTermoConsentimento",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

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

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmouQuestionarioSaude",
                table: "AceitesTermoConsentimento",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConviteId",
                table: "AceitesTermoConsentimento",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoIp",
                table: "AceitesTermoConsentimento",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenciaHash",
                table: "AceitesTermoConsentimento",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "0000000000000000000000000000000000000000000000000000000000000000");

            migrationBuilder.AddColumn<string>(
                name: "EvidenciaJson",
                table: "AceitesTermoConsentimento",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{\"registroLegado\":true}");

            migrationBuilder.AddColumn<int>(
                name: "VersaoEvidencia",
                table: "AceitesTermoConsentimento",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_AceitesTermoConsentimento_ConviteId",
                table: "AceitesTermoConsentimento",
                column: "ConviteId");

            migrationBuilder.AddForeignKey(
                name: "FK_AceitesTermoConsentimento_ConvitesFicha_ConviteId",
                table: "AceitesTermoConsentimento",
                column: "ConviteId",
                principalTable: "ConvitesFicha",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AceitesTermoConsentimento_ConvitesFicha_ConviteId",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropIndex(
                name: "IX_AceitesTermoConsentimento_ConviteId",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "AgenteUsuario",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "ConfirmouDadosPessoais",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "ConfirmouMaioridade",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "ConfirmouQuestionarioSaude",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "ConviteId",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "EnderecoIp",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "EvidenciaHash",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "EvidenciaJson",
                table: "AceitesTermoConsentimento");

            migrationBuilder.DropColumn(
                name: "VersaoEvidencia",
                table: "AceitesTermoConsentimento");
        }
    }
}

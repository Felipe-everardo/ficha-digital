using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalAndProcedureToRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProfissionalResponsavelId",
                table: "Fichas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfissionalResponsavelNome",
                table: "Fichas",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "Profissional não informado");

            migrationBuilder.AddColumn<string>(
                name: "TipoProcedimento",
                table: "Fichas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "NaoInformado");

            migrationBuilder.AddColumn<int>(
                name: "Especialidades",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Fichas_ProfissionalResponsavelId",
                table: "Fichas",
                column: "ProfissionalResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Fichas_Status",
                table: "Fichas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Fichas_TipoProcedimento",
                table: "Fichas",
                column: "TipoProcedimento");

            migrationBuilder.AddForeignKey(
                name: "FK_Fichas_AspNetUsers_ProfissionalResponsavelId",
                table: "Fichas",
                column: "ProfissionalResponsavelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fichas_AspNetUsers_ProfissionalResponsavelId",
                table: "Fichas");

            migrationBuilder.DropIndex(
                name: "IX_Fichas_ProfissionalResponsavelId",
                table: "Fichas");

            migrationBuilder.DropIndex(
                name: "IX_Fichas_Status",
                table: "Fichas");

            migrationBuilder.DropIndex(
                name: "IX_Fichas_TipoProcedimento",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "ProfissionalResponsavelId",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "ProfissionalResponsavelNome",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "TipoProcedimento",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "Especialidades",
                table: "AspNetUsers");
        }
    }
}

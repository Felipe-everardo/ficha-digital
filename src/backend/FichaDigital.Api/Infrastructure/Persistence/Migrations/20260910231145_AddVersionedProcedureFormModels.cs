using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionedProcedureFormModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CnpjApresentado",
                table: "Fichas",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersaoModelo",
                table: "Fichas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersaoQuestionario",
                table: "Fichas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersaoTermo",
                table: "Fichas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CnpjApresentado",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "VersaoModelo",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "VersaoQuestionario",
                table: "Fichas");

            migrationBuilder.DropColumn(
                name: "VersaoTermo",
                table: "Fichas");
        }
    }
}

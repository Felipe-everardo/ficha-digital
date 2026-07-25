using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPronomesAndClienteIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pronomes",
                table: "Clientes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Celular",
                table: "Clientes",
                column: "Celular");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NomeCompleto",
                table: "Clientes",
                column: "NomeCompleto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_Celular",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_NomeCompleto",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Pronomes",
                table: "Clientes");
        }
    }
}

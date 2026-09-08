using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RegistradaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Despesas_AspNetUsers_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_Categoria",
                table: "Despesas",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_Data",
                table: "Despesas",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_ProfissionalId",
                table: "Despesas",
                column: "ProfissionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Despesas");
        }
    }
}

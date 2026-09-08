using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceFinancialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Atendimentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataRealizacao = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorCobrado = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Desconto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorFinal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SituacaoPagamento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RegistradoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atendimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Atendimentos_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_DataRealizacao",
                table: "Atendimentos",
                column: "DataRealizacao");

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_FichaId",
                table: "Atendimentos",
                column: "FichaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_SituacaoPagamento",
                table: "Atendimentos",
                column: "SituacaoPagamento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atendimentos");
        }
    }
}

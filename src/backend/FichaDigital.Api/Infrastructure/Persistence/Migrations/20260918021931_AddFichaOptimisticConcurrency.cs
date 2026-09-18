using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFichaOptimisticConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VersaoConcorrencia",
                table: "Fichas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VersaoConcorrencia",
                table: "Fichas");
        }
    }
}

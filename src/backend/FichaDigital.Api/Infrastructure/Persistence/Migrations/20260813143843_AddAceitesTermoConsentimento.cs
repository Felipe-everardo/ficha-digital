using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAceitesTermoConsentimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AceitesTermoConsentimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersaoTermo = table.Column<int>(type: "int", nullable: false),
                    ConteudoTermo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConteudoHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NomeAssinante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AceitoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AceitesTermoConsentimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AceitesTermoConsentimento_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AceitesTermoConsentimento_FichaId",
                table: "AceitesTermoConsentimento",
                column: "FichaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AceitesTermoConsentimento");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionariosSaude : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionariosSaude",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Versao = table.Column<int>(type: "int", nullable: false),
                    TemDiabetes = table.Column<bool>(type: "bit", nullable: false),
                    TipoDiabetes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PossuiPressaoAlta = table.Column<bool>(type: "bit", nullable: false),
                    TemAlergia = table.Column<bool>(type: "bit", nullable: false),
                    DescricaoAlergia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EstaGravidaOuAmamentando = table.Column<bool>(type: "bit", nullable: false),
                    RespondidoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionariosSaude", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionariosSaude_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionariosSaude_FichaId",
                table: "QuestionariosSaude",
                column: "FichaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionariosSaude");
        }
    }
}

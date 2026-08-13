using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCondicoesClinicasQuestionarioSaude : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PossuiCondicaoCardiaca",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TemEpilepsia",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TemHemofilia",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsaMarcaPasso",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PossuiCondicaoCardiaca",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "TemEpilepsia",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "TemHemofilia",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "UsaMarcaPasso",
                table: "QuestionariosSaude");
        }
    }
}

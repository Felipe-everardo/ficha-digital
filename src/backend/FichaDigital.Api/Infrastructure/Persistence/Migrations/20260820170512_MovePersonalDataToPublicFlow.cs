using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MovePersonalDataToPublicFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NomeCompleto",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataNascimento",
                table: "Clientes",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "Celular",
                table: "Clientes",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<string>(
                name: "ContatoEmergenciaCelular",
                table: "Clientes",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContatoEmergenciaNome",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DadosPessoaisPreenchidosEmUtc",
                table: "Clientes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeReferencia",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE [Clientes]
                SET [NomeReferencia] = [NomeCompleto],
                    [DadosPessoaisPreenchidosEmUtc] = [CriadoEmUtc]
                WHERE [NomeReferencia] = N'';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NomeReferencia",
                table: "Clientes",
                column: "NomeReferencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Clientes]
                SET [NomeCompleto] = COALESCE([NomeCompleto], [NomeReferencia]),
                    [DataNascimento] = COALESCE([DataNascimento], '1900-01-01'),
                    [Celular] = COALESCE([Celular], N'Não informado');
                """);

            migrationBuilder.DropIndex(
                name: "IX_Clientes_NomeReferencia",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ContatoEmergenciaCelular",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ContatoEmergenciaNome",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DadosPessoaisPreenchidosEmUtc",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NomeReferencia",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "NomeCompleto",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataNascimento",
                table: "Clientes",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Celular",
                table: "Clientes",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25,
                oldNullable: true);
        }
    }
}

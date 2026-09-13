using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyStudioAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_CriadoPorProfissionalId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CriadoPorProfissionalId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Especialidades",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CriadoPorProfissionalId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Especialidades",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CriadoPorProfissionalId",
                table: "Clientes",
                column: "CriadoPorProfissionalId");
        }
    }
}

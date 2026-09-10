using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FocusFichaCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atendimentos");

            migrationBuilder.DropTable(
                name: "Despesas");

            migrationBuilder.CreateTable(
                name: "DadosPessoaisFichas",
                columns: table => new
                {
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NomeSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Pronomes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContatoEmergenciaNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContatoEmergenciaCelular = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ConfirmadosEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DadosPessoaisFichas", x => x.FichaId);
                    table.ForeignKey(
                        name: "FK_DadosPessoaisFichas_Fichas_FichaId",
                        column: x => x.FichaId,
                        principalTable: "Fichas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Preserva a melhor aproximação possível para fichas antigas.
            // Antes desta migração, os dados pessoais existiam apenas no
            // cadastro mutável do cliente e não havia um retrato por ficha.
            migrationBuilder.Sql(
                """
                INSERT INTO [DadosPessoaisFichas] (
                    [FichaId], [NomeCompleto], [NomeSocial], [Pronomes],
                    [DataNascimento], [Celular], [Email], [Instagram],
                    [ContatoEmergenciaNome], [ContatoEmergenciaCelular],
                    [ConfirmadosEmUtc])
                SELECT
                    [f].[Id], [c].[NomeCompleto], [c].[NomeSocial], [c].[Pronomes],
                    [c].[DataNascimento], [c].[Celular], [c].[Email], [c].[Instagram],
                    [c].[ContatoEmergenciaNome], [c].[ContatoEmergenciaCelular],
                    [c].[DadosPessoaisPreenchidosEmUtc]
                FROM [Fichas] AS [f]
                INNER JOIN [Clientes] AS [c] ON [c].[Id] = [f].[ClienteId]
                WHERE [c].[DadosPessoaisPreenchidosEmUtc] IS NOT NULL
                  AND [c].[NomeCompleto] IS NOT NULL
                  AND [c].[DataNascimento] IS NOT NULL
                  AND [c].[Celular] IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DadosPessoaisFichas");

            migrationBuilder.CreateTable(
                name: "Atendimentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtualizadoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataRealizacao = table.Column<DateOnly>(type: "date", nullable: false),
                    Desconto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FichaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RegistradoEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SituacaoPagamento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ValorCobrado = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorFinal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtualizadaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfissionalNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RegistradaEmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
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
    }
}

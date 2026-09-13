using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRealFormPersonalAndHealthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConsumiuBebidaAlcoolicaUltimas24Horas",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DescricaoAnemia",
                table: "QuestionariosSaude",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescricaoDoencaTransmissivel",
                table: "QuestionariosSaude",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescricaoMedicacao",
                table: "QuestionariosSaude",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Fuma",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PossuiDoencaTransmissivel",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TeveAnemia",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TeveHepatite",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TipoHepatite",
                table: "QuestionariosSaude",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsaMedicacao",
                table: "QuestionariosSaude",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "DadosPessoaisFichas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "DadosPessoaisFichas",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "DadosPessoaisFichas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "DadosPessoaisFichas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "DadosPessoaisFichas",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "DadosPessoaisFichas",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "DadosPessoaisFichas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "DadosPessoaisFichas",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "DadosPessoaisFichas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefoneAdicional",
                table: "DadosPessoaisFichas",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "Clientes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Clientes",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Clientes",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Clientes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefoneAdicional",
                table: "Clientes",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ConsumiuBebidaAlcoolicaUltimas24Horas",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "DescricaoAnemia",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "DescricaoDoencaTransmissivel",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "DescricaoMedicacao",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "Fuma",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "PossuiDoencaTransmissivel",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "TeveAnemia",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "TeveHepatite",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "TipoHepatite",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "UsaMedicacao",
                table: "QuestionariosSaude");

            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "TelefoneAdicional",
                table: "DadosPessoaisFichas");

            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TelefoneAdicional",
                table: "Clientes");
        }
    }
}

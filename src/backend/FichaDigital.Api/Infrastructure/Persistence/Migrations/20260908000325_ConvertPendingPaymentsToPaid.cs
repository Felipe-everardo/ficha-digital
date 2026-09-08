using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FichaDigital.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPendingPaymentsToPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Atendimentos]
                SET [SituacaoPagamento] = N'Pago'
                WHERE [SituacaoPagamento] = N'Pendente';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A situação original não pode ser reconstruída com segurança.
        }
    }
}

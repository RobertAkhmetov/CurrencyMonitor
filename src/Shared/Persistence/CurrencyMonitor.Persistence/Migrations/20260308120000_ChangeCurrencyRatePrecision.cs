using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurrencyMonitor.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCurrencyRatePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "currency",
                type: "numeric(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "currency",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,4)",
                oldPrecision: 10,
                oldScale: 4);
        }
    }
}

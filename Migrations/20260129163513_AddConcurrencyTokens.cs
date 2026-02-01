using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NonProfitFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "RowVersion",
                table: "Transactions",
                type: "INTEGER",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "RowVersion",
                table: "Grants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "RowVersion",
                table: "Funds",
                type: "INTEGER",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Funds");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NonProfitFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddFundStartingBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StartingBalance",
                table: "Funds",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartingBalance",
                table: "Funds");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NonProfitFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToFundId",
                table: "Transactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferPairId",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ToFundId",
                table: "Transactions",
                column: "ToFundId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Funds_ToFundId",
                table: "Transactions",
                column: "ToFundId",
                principalTable: "Funds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Funds_ToFundId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ToFundId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ToFundId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransferPairId",
                table: "Transactions");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NonProfitFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddImportPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ImportType = table.Column<string>(type: "TEXT", nullable: false),
                    DateColumn = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountColumn = table.Column<int>(type: "INTEGER", nullable: false),
                    DescriptionColumn = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeColumn = table.Column<int>(type: "INTEGER", nullable: true),
                    CategoryColumn = table.Column<int>(type: "INTEGER", nullable: true),
                    FundColumn = table.Column<int>(type: "INTEGER", nullable: true),
                    DonorColumn = table.Column<int>(type: "INTEGER", nullable: true),
                    GrantColumn = table.Column<int>(type: "INTEGER", nullable: true),
                    PayeeColumn = table.Column<int>(type: "INTEGER", nullable: true),
                    TagsColumn = table.Column<int>(type: "INTEGER", nullable: true),
                    HasHeaderRow = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateFormat = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportPresets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FundId_Date",
                table: "Transactions",
                columns: new[] { "FundId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Payee",
                table: "Transactions",
                column: "Payee");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PONumber",
                table: "Transactions",
                column: "PONumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReferenceNumber",
                table: "Transactions",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donors_Email",
                table: "Donors",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsArchived",
                table: "Documents",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OriginalFileName",
                table: "Documents",
                column: "OriginalFileName");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Type",
                table: "Documents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedAt",
                table: "Documents",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportPresets");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_FundId_Date",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Payee",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PONumber",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReferenceNumber",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Funds_Name",
                table: "Funds");

            migrationBuilder.DropIndex(
                name: "IX_Donors_Email",
                table: "Donors");

            migrationBuilder.DropIndex(
                name: "IX_Documents_IsArchived",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_OriginalFileName",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Type",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_UploadedAt",
                table: "Documents");
        }
    }
}

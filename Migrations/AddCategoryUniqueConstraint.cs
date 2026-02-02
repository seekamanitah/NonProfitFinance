using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NonProfitFinance.Migrations
{
    /// <summary>
    /// Adds case-insensitive unique constraint for category names.
    /// Prevents duplicate categories like "Dinners" and "dinners" at the same hierarchy level.
    /// 
    /// IMPORTANT: Before applying this migration, ensure no duplicate categories exist.
    /// Run: fix_duplicate_categories.sql to clean up duplicates.
    /// </summary>
    public partial class AddCategoryUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop existing case-sensitive unique index
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS IX_Categories_ParentId_Name;
            ");

            // Step 2: Create case-insensitive unique index
            // - LOWER(Name) ensures case-insensitive comparison
            // - COALESCE(ParentId, -1) handles NULL values (SQLite treats NULL != NULL)
            // This allows:
            //   - "Supplies" under parent 1
            //   - "Supplies" under parent 2
            // But prevents:
            //   - "Dinners" and "dinners" at same level
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_Categories_Name_Lower 
                ON Categories (LOWER(Name), COALESCE(ParentId, -1));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the case-insensitive index
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS IX_Categories_Name_Lower;
            ");

            // Restore original case-sensitive index
            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId_Name",
                table: "Categories",
                columns: new[] { "ParentId", "Name" },
                unique: true);
        }
    }
}

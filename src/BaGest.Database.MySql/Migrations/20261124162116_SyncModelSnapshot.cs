using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaGest.Database.MySql.Migrations
{
    /// <summary>
    /// Brings the model snapshot in line with the current provider, which marks the integer <c>Key</c>
    /// columns with <c>MySql:ValueGenerationStrategy = IdentityColumn</c>. The snapshot was generated
    /// before that annotation existed, so EF Core reported pending model changes on every startup.
    /// </summary>
    /// <remarks>
    /// Intentionally empty: the <c>Key</c> columns have been <c>AUTO_INCREMENT</c> since the initial
    /// migration, so the <c>ALTER TABLE ... MODIFY COLUMN</c> statements EF Core would generate here
    /// would only rebuild the tables without changing anything.
    /// </remarks>
    public partial class SyncModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}

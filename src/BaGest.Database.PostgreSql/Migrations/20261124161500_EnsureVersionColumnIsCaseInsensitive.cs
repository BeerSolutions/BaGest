using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaGest.Database.PostgreSql.Migrations
{
    /// <summary>
    /// The "FixVersionCaseSensitivity" migration did not emit any SQL for the "Version" column
    /// with current Npgsql versions, leaving it as "character varying(64)" instead of "citext".
    /// This converts the column when needed and is a no-op for databases that already use "citext".
    /// Note: rows for the same package id whose versions differ only by letter case would violate
    /// the unique index once the column is case insensitive; versions are stored normalized, so this is not expected.
    /// See: https://github.com/bagetter/BaGetter/issues/239
    /// </summary>
    public partial class EnsureVersionColumnIsCaseInsensitive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'Packages'
                          AND column_name = 'Version'
                          AND udt_name <> 'citext')
                    THEN
                        ALTER TABLE "Packages" ALTER COLUMN "Version" TYPE citext;
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: the column is meant to be case insensitive.
        }
    }
}

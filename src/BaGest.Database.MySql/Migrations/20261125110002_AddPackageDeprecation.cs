using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaGest.Database.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageDeprecation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeprecationAlternatePackageId",
                table: "Packages",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeprecationAlternatePackageVersionRange",
                table: "Packages",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeprecationMessage",
                table: "Packages",
                type: "longtext",
                maxLength: 4000,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DeprecationReasons",
                table: "Packages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeprecationAlternatePackageId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeprecationAlternatePackageVersionRange",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeprecationMessage",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeprecationReasons",
                table: "Packages");
        }
    }
}

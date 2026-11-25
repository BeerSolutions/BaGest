using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaGest.Database.PostgreSql.Migrations
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
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeprecationAlternatePackageVersionRange",
                table: "Packages",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeprecationMessage",
                table: "Packages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeprecationReasons",
                table: "Packages",
                type: "integer",
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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaGest.Database.MySql.Migrations
{
    /// <summary>
    /// Converts every text column from latin1 to utf8mb4, so package metadata with characters outside latin1
    /// (for example authors such as "Jan Havlíček") can be stored. Existing values are converted by the server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The columns limited to 4000 characters become longtext: as utf8mb4 varchar(4000) columns, the Packages table
    /// would exceed MySQL's 65,535-byte row size limit. utf8mb4_unicode_ci keeps package IDs and versions
    /// case-insensitive, as they were with latin1_swedish_ci.
    /// </para>
    /// <para>
    /// Each table is converted by a single ALTER TABLE stating the full target definition of its text columns.
    /// MySQL does not run DDL in a transaction, so every statement leaves the table in a consistent state and can
    /// be run again. The tables are set to ROW_FORMAT=DYNAMIC, the default since MySQL 5.7.9 and MariaDB 10.2.2,
    /// because the utf8mb4 indexes exceed the 767-byte key limit of the older COMPACT format.
    /// </para>
    /// <para>
    /// The server copies each table, so on a large database the migration takes a while and blocks writes to the
    /// table meanwhile. utf8mb4_unicode_ci also ignores accents, so it fails with a duplicate key error in the
    /// unlikely case that two packages have the same version and IDs differing only in accents.
    /// </para>
    /// </remarks>
    public partial class UseCharSetUtf8mb4 : Migration
    {
        private const string Utf8mb4 = "CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
        private const string Latin1 = "CHARACTER SET latin1 COLLATE latin1_swedish_ci";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ConvertDatabase(migrationBuilder, Utf8mb4, longStringType: "longtext");
        }

        /// <inheritdoc />
        /// <remarks>
        /// Values that latin1 cannot represent, or that are longer than 4000 characters, cannot be converted back.
        /// In strict SQL mode (the default) the server then fails the migration; otherwise it replaces such
        /// characters with "?" and truncates such values. Change them before reverting this migration.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ConvertDatabase(migrationBuilder, Latin1, longStringType: "varchar(4000)");
        }

        private static void ConvertDatabase(MigrationBuilder migrationBuilder, string charSet, string longStringType)
        {
            var tableOptions = $"{charSet} ROW_FORMAT=DYNAMIC";

            migrationBuilder.Sql($"ALTER DATABASE {charSet};");

            migrationBuilder.Sql($"""
                ALTER TABLE `Packages`
                    MODIFY `Id` varchar(128) {charSet} NOT NULL,
                    MODIFY `Version` varchar(64) {charSet} NOT NULL,
                    MODIFY `OriginalVersion` varchar(64) {charSet} NULL,
                    MODIFY `Language` varchar(20) {charSet} NULL,
                    MODIFY `MinClientVersion` varchar(44) {charSet} NULL,
                    MODIFY `Title` varchar(256) {charSet} NULL,
                    MODIFY `RepositoryType` varchar(100) {charSet} NULL,
                    MODIFY `Authors` {longStringType} {charSet} NULL,
                    MODIFY `Description` {longStringType} {charSet} NULL,
                    MODIFY `Summary` {longStringType} {charSet} NULL,
                    MODIFY `Tags` {longStringType} {charSet} NULL,
                    MODIFY `IconUrl` {longStringType} {charSet} NULL,
                    MODIFY `LicenseUrl` {longStringType} {charSet} NULL,
                    MODIFY `ProjectUrl` {longStringType} {charSet} NULL,
                    MODIFY `RepositoryUrl` {longStringType} {charSet} NULL,
                    MODIFY `CachedFrom` {longStringType} {charSet} NULL,
                    MODIFY `ReleaseNotes` longtext {charSet} NULL,
                    {tableOptions};
                """);

            migrationBuilder.Sql($"""
                ALTER TABLE `PackageDependencies`
                    MODIFY `Id` varchar(128) {charSet} NULL,
                    MODIFY `VersionRange` varchar(256) {charSet} NULL,
                    MODIFY `TargetFramework` varchar(256) {charSet} NULL,
                    {tableOptions};
                """);

            migrationBuilder.Sql($"""
                ALTER TABLE `PackageTypes`
                    MODIFY `Name` varchar(512) {charSet} NULL,
                    MODIFY `Version` varchar(64) {charSet} NULL,
                    {tableOptions};
                """);

            migrationBuilder.Sql($"""
                ALTER TABLE `TargetFrameworks`
                    MODIFY `Moniker` varchar(256) {charSet} NULL,
                    {tableOptions};
                """);
        }
    }
}

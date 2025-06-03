using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Main.Migrations
{
    /// <inheritdoc />
    public partial class AddingSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cnf");

            migrationBuilder.EnsureSchema(
                name: "ath");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "UserRoles",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "UserCompanies",
                newName: "UserCompanies",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "Settlements",
                newName: "Settlements",
                newSchema: "cnf");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Roles",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "RolePermissions",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "RegistrationRequests",
                newName: "RegistrationRequests",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "Regions",
                newName: "Regions",
                newSchema: "cnf");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefreshTokens",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "Municipalities",
                newName: "Municipalities",
                newSchema: "cnf");

            migrationBuilder.RenameTable(
                name: "Licenses",
                newName: "Licenses",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "LicenseFeature",
                newName: "LicenseFeature",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "Countries",
                newSchema: "cnf");

            migrationBuilder.RenameTable(
                name: "Companies",
                newName: "Companies",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "ApiKeys",
                newName: "ApiKeys",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "ApiKeyPermissions",
                newName: "ApiKeyPermissions",
                newSchema: "ath");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "Addresses",
                newSchema: "cnf");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                schema: "ath",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "ath",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "UserCompanies",
                schema: "ath",
                newName: "UserCompanies");

            migrationBuilder.RenameTable(
                name: "Settlements",
                schema: "cnf",
                newName: "Settlements");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "ath",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "ath",
                newName: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "RegistrationRequests",
                schema: "ath",
                newName: "RegistrationRequests");

            migrationBuilder.RenameTable(
                name: "Regions",
                schema: "cnf",
                newName: "Regions");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                schema: "ath",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "Municipalities",
                schema: "cnf",
                newName: "Municipalities");

            migrationBuilder.RenameTable(
                name: "Licenses",
                schema: "ath",
                newName: "Licenses");

            migrationBuilder.RenameTable(
                name: "LicenseFeature",
                schema: "ath",
                newName: "LicenseFeature");

            migrationBuilder.RenameTable(
                name: "Countries",
                schema: "cnf",
                newName: "Countries");

            migrationBuilder.RenameTable(
                name: "Companies",
                schema: "ath",
                newName: "Companies");

            migrationBuilder.RenameTable(
                name: "ApiKeys",
                schema: "ath",
                newName: "ApiKeys");

            migrationBuilder.RenameTable(
                name: "ApiKeyPermissions",
                schema: "ath",
                newName: "ApiKeyPermissions");

            migrationBuilder.RenameTable(
                name: "Addresses",
                schema: "cnf",
                newName: "Addresses");
        }
    }
}

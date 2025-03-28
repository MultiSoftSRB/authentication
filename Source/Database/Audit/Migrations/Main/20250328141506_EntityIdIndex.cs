using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Audit.Migrations.Main
{
    /// <inheritdoc />
    public partial class EntityIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_EntityId",
                table: "Users",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_EntityId",
                table: "UserRoles",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanies_EntityId",
                table: "UserCompanies",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_EntityId",
                table: "Roles",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_EntityId",
                table: "RolePermissions",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultAuditLogs_EntityId",
                table: "DefaultAuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_EntityId",
                table: "Companies",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_EntityId",
                table: "ApiKeys",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyPermissions_EntityId",
                table: "ApiKeyPermissions",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyClients_EntityId",
                table: "AgencyClients",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EntityId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_EntityId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserCompanies_EntityId",
                table: "UserCompanies");

            migrationBuilder.DropIndex(
                name: "IX_Roles_EntityId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_EntityId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_DefaultAuditLogs_EntityId",
                table: "DefaultAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Companies_EntityId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_EntityId",
                table: "ApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeyPermissions_EntityId",
                table: "ApiKeyPermissions");

            migrationBuilder.DropIndex(
                name: "IX_AgencyClients_EntityId",
                table: "AgencyClients");
        }
    }
}

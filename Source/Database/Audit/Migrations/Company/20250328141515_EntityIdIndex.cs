using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Audit.Migrations.Company
{
    /// <inheritdoc />
    public partial class EntityIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DefaultAuditLogs_EntityId",
                table: "DefaultAuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleAuditLogs_EntityId",
                table: "ArticleAuditLogs",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DefaultAuditLogs_EntityId",
                table: "DefaultAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_ArticleAuditLogs_EntityId",
                table: "ArticleAuditLogs");
        }
    }
}

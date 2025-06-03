using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Audit.Migrations.Main
{
    /// <inheritdoc />
    public partial class RemovingCompanyAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleAuditLogs_CompanyId",
                table: "ArticleAuditLogs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleAuditLogs_EntityId",
                table: "ArticleAuditLogs",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleAuditLogs");
        }
    }
}

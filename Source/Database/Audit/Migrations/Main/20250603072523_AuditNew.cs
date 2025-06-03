using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Audit.Migrations.Main
{
    /// <inheritdoc />
    public partial class AuditNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleAuditLogs",
                table: "ArticleAuditLogs");

            migrationBuilder.RenameTable(
                name: "ArticleAuditLogs",
                newName: "Articles");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleAuditLogs_EntityId",
                table: "Articles",
                newName: "IX_Articles_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleAuditLogs_CompanyId",
                table: "Articles",
                newName: "IX_Articles_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Articles",
                table: "Articles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Addresses",
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
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
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
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipalities",
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
                    table.PrimaryKey("PK_Municipalities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
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
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
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
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CompanyId",
                table: "Addresses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_EntityId",
                table: "Addresses",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CompanyId",
                table: "Countries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_EntityId",
                table: "Countries",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_CompanyId",
                table: "Municipalities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_EntityId",
                table: "Municipalities",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CompanyId",
                table: "Regions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_EntityId",
                table: "Regions",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_CompanyId",
                table: "Settlements",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_EntityId",
                table: "Settlements",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Articles",
                table: "Articles");

            migrationBuilder.RenameTable(
                name: "Articles",
                newName: "ArticleAuditLogs");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_EntityId",
                table: "ArticleAuditLogs",
                newName: "IX_ArticleAuditLogs_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_CompanyId",
                table: "ArticleAuditLogs",
                newName: "IX_ArticleAuditLogs_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleAuditLogs",
                table: "ArticleAuditLogs",
                column: "Id");
        }
    }
}

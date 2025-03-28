using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Audit.Migrations.Main
{
    /// <inheritdoc />
    public partial class MainAuditInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "AuditLogSequence");

            migrationBuilder.CreateTable(
                name: "AgencyClients",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeyPermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefaultAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCompanies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"AuditLogSequence\"')"),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApiKeyId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyClients_CompanyId",
                table: "AgencyClients",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyPermissions_CompanyId",
                table: "ApiKeyPermissions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_CompanyId",
                table: "ApiKeys",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CompanyId",
                table: "Companies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultAuditLogs_CompanyId",
                table: "DefaultAuditLogs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CompanyId",
                table: "RolePermissions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CompanyId",
                table: "Roles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanies_CompanyId",
                table: "UserCompanies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_CompanyId",
                table: "UserRoles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyClients");

            migrationBuilder.DropTable(
                name: "ApiKeyPermissions");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "DefaultAuditLogs");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserCompanies");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropSequence(
                name: "AuditLogSequence");
        }
    }
}

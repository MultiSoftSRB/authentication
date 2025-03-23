using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Main.Migrations
{
    /// <inheritdoc />
    public partial class AgencyClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessType",
                table: "UserCompanies",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "AgencyCompanyId",
                table: "UserCompanies",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgencyClients",
                columns: table => new
                {
                    AgencyCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    ClientCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyClients", x => new { x.AgencyCompanyId, x.ClientCompanyId });
                    table.ForeignKey(
                        name: "FK_AgencyClients_Companies_AgencyCompanyId",
                        column: x => x.AgencyCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgencyClients_Companies_ClientCompanyId",
                        column: x => x.ClientCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanies_AgencyCompanyId",
                table: "UserCompanies",
                column: "AgencyCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyClients_ClientCompanyId",
                table: "AgencyClients",
                column: "ClientCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompanies_Companies_AgencyCompanyId",
                table: "UserCompanies",
                column: "AgencyCompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCompanies_Companies_AgencyCompanyId",
                table: "UserCompanies");

            migrationBuilder.DropTable(
                name: "AgencyClients");

            migrationBuilder.DropIndex(
                name: "IX_UserCompanies_AgencyCompanyId",
                table: "UserCompanies");

            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "UserCompanies");

            migrationBuilder.DropColumn(
                name: "AgencyCompanyId",
                table: "UserCompanies");
        }
    }
}

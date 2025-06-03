using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Main.Migrations
{
    /// <inheritdoc />
    public partial class DoubleFKIdsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Settlements_SettlementId1",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Municipalities_Regions_RegionId1",
                table: "Municipalities");

            migrationBuilder.DropForeignKey(
                name: "FK_Settlements_Municipalities_MunicipalityId1",
                table: "Settlements");

            migrationBuilder.DropIndex(
                name: "IX_Settlements_MunicipalityId1",
                table: "Settlements");

            migrationBuilder.DropIndex(
                name: "IX_Municipalities_RegionId1",
                table: "Municipalities");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_SettlementId1",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "MunicipalityId1",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "RegionId1",
                table: "Municipalities");

            migrationBuilder.DropColumn(
                name: "SettlementId1",
                table: "Addresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MunicipalityId1",
                table: "Settlements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegionId1",
                table: "Municipalities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SettlementId1",
                table: "Addresses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_MunicipalityId1",
                table: "Settlements",
                column: "MunicipalityId1");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_RegionId1",
                table: "Municipalities",
                column: "RegionId1");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_SettlementId1",
                table: "Addresses",
                column: "SettlementId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Settlements_SettlementId1",
                table: "Addresses",
                column: "SettlementId1",
                principalTable: "Settlements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Municipalities_Regions_RegionId1",
                table: "Municipalities",
                column: "RegionId1",
                principalTable: "Regions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Settlements_Municipalities_MunicipalityId1",
                table: "Settlements",
                column: "MunicipalityId1",
                principalTable: "Municipalities",
                principalColumn: "Id");
        }
    }
}

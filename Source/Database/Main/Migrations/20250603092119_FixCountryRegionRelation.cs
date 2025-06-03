using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Main.Migrations
{
    /// <inheritdoc />
    public partial class FixCountryRegionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Countries_CountryId1",
                table: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Regions_CountryId1",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                table: "Regions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CountryId1",
                table: "Regions",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CountryId1",
                table: "Regions",
                column: "CountryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Countries_CountryId1",
                table: "Regions",
                column: "CountryId1",
                principalTable: "Countries",
                principalColumn: "Id");
        }
    }
}

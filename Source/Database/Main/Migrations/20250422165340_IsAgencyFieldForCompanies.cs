using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSoftSRB.Database.Main.Migrations
{
    /// <inheritdoc />
    public partial class IsAgencyFieldForCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAgency",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAgency",
                table: "Companies");
        }
    }
}

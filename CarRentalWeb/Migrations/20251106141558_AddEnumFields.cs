using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEnumFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Rentals");
        }
    }
}

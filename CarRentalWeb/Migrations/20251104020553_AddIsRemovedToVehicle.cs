using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRemovedToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                table: "Vehicles");
        }
    }
}

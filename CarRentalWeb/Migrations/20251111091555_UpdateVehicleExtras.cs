using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TruckType",
                table: "Vehicles",
                newName: "Transmission");

            migrationBuilder.RenameColumn(
                name: "CarType",
                table: "Vehicles",
                newName: "MileagePolicy");

            migrationBuilder.RenameColumn(
                name: "BikeType",
                table: "Vehicles",
                newName: "Luggage");

            migrationBuilder.AddColumn<string>(
                name: "Bike_Transmission",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Car_HasAC",
                table: "Vehicles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Car_Transmission",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAC",
                table: "Vehicles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCarPlay",
                table: "Vehicles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasStorageBox",
                table: "Vehicles",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bike_Transmission",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Car_HasAC",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Car_Transmission",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasAC",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasCarPlay",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasStorageBox",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "Transmission",
                table: "Vehicles",
                newName: "TruckType");

            migrationBuilder.RenameColumn(
                name: "MileagePolicy",
                table: "Vehicles",
                newName: "CarType");

            migrationBuilder.RenameColumn(
                name: "Luggage",
                table: "Vehicles",
                newName: "BikeType");
        }
    }
}

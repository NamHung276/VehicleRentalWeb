using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalAndCustomerSourceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegistrationType",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationType",
                table: "Customers");
        }
    }
}

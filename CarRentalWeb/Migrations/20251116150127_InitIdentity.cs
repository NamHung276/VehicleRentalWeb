using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_User_UserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_User_UserId1",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Customers",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId1",
                table: "Customers",
                column: "UserId1",
                unique: true,
                filter: "[UserId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_User_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_User_UserId1",
                table: "Customers",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}

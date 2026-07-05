using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Play_Zone_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Games",
                table: "Devices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Games",
                table: "Devices");
        }
    }
}

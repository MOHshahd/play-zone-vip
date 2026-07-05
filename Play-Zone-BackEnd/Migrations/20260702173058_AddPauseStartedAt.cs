using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Play_Zone_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddPauseStartedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PauseStartedAt",
                table: "Sessions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PauseStartedAt",
                table: "Sessions");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class addGeoChecktoAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AttendanceConfirmed",
                table: "Bookings",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceConfirmedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceConfirmed",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AttendanceConfirmedAt",
                table: "Bookings");
        }
    }
}

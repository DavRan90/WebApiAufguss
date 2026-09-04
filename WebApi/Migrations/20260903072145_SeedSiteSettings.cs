using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "AufgussHiddenDaysInAdvance", "MaxBookingsAufguss", "MaxBookingsVattenfys", "MaxNews", "VattenfysHiddenDaysInAdvance" },
                values: new object[] { 1, 7, 2, 5, 5, 7 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}

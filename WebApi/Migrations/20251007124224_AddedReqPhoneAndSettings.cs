using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedReqPhoneAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AufgussFriendId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaxBookingsAufguss = table.Column<int>(type: "int", nullable: false),
                    AufgussHiddenDaysInAdvance = table.Column<int>(type: "int", nullable: false),
                    MaxBookingsVattenfys = table.Column<int>(type: "int", nullable: false),
                    VattenfysHiddenDaysInAdvance = table.Column<int>(type: "int", nullable: false),
                    MaxNews = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "AufgussFriendId",
                table: "AspNetUsers");
        }
    }
}

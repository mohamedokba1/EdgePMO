using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AlterContentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "CourseVideos",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CourseVideos",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "CourseVideos",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CourseVideos",
                newName: "Id");
        }
    }
}

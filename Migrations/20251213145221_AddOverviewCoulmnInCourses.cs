using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOverviewCoulmnInCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Overview",
                table: "Courses",
                newName: "Overview");

            migrationBuilder.AddColumn<string>(
                name: "MainObjective",
                table: "Courses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Overview",
                table: "Courses",
                newName: "Overview");
            migrationBuilder.DropColumn(
                name: "MainObjective",
                table: "Courses");
        }
    }
}

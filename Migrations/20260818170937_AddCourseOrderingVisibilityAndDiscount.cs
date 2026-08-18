using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseOrderingVisibilityAndDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: hand-corrected after generation — the scaffolded migration defaulted
            // both booleans to `false`, which would have flipped every existing course to
            // hidden and hidden every student count the moment this applied, even though
            // the C# model's `= true` initializer says otherwise (a C# field initializer
            // is not a SQL DEFAULT unless configured via Fluent API, so the generator fell
            // back to bool's CLR default). Existing rows must stay visible.
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "OriginalPrice",
                table: "Courses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowStudentsCount",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ShowStudentsCount",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Courses");
        }
    }
}

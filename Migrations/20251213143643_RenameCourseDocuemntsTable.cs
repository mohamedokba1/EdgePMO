using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameCourseDocuemntsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the table from CourseDocuemnts to CourseDocuments
            migrationBuilder.RenameTable(
                name: "CourseDocuemnts",
                newName: "CourseDocuments");

            // Rename the indexes if they exist
            migrationBuilder.RenameIndex(
                name: "IX_CourseDocuemnts_CourseOutlineId",
                table: "CourseDocuments",
                newName: "IX_CourseDocuments_CourseOutlineId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseDocuemnts_DocumentUrl",
                table: "CourseDocuments",
                newName: "IX_CourseDocuments_DocumentUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename back to CourseDocuemnts
            migrationBuilder.RenameTable(
                name: "CourseDocuments",
                newName: "CourseDocuemnts");

            // Rename indexes back
            migrationBuilder.RenameIndex(
                name: "IX_CourseDocuments_CourseOutlineId",
                table: "CourseDocuemnts",
                newName: "IX_CourseDocuemnts_CourseOutlineId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseDocuments_DocumentUrl",
                table: "CourseDocuemnts",
                newName: "IX_CourseDocuemnts_DocumentUrl");
        }
    }
}

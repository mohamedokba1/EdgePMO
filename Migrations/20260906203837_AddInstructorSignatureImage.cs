using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorSignatureImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureImageUrl",
                table: "Instructors",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureImageUrl",
                table: "Instructors");
        }
    }
}

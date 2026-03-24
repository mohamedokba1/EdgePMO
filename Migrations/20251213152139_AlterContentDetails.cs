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
                name: "VideoUrl",
                table: "CourseVideos",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "CertificateId",
                table: "CourseVideos",
                newName: "CertificateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "CourseVideos",
                newName: "VideoUrl");

            migrationBuilder.RenameColumn(
                name: "CertificateId",
                table: "CourseVideos",
                newName: "CertificateId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameConsultationRequestsFields : Migration
    {
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Company",
                table: "ConsultationRequests",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "ConsultationRequests",
                newName: "EmailAddress");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "ConsultationRequests",
                newName: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "ConsultationRequests",
                newName: "Company");

            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                table: "ConsultationRequests",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "ConsultationRequests",
                newName: "Phone");
        }
    }
}
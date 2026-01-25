using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AlterDeleteBehaviorForPurchaseRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_Courses_CourseId",
                table: "PurchaseRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_Courses_CourseId",
                table: "PurchaseRequests",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_Courses_CourseId",
                table: "PurchaseRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_Courses_CourseId",
                table: "PurchaseRequests",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }
    }
}

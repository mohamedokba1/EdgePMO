using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class FixForiegnKeysIssueOnReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseId1",
                table: "CourseReviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseReviews_CourseId1",
                table: "CourseReviews",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseReviews_Courses_CourseId1",
                table: "CourseReviews",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }
    }
}

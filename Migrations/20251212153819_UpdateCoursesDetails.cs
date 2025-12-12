using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCoursesDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Instructors_InstructorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "WhatStudentsLearn",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "WhatStudentsLearn",
                table: "Courses",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]"
                );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Courses",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CoursePictureUrl",
                table: "Courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Courses",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Certification",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LongDescription",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Courses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "Sessions",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SoftwareUsed",
                table: "Courses",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "Students",
                table: "Courses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "Courses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhoShouldAttend",
                table: "Courses",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "CourseOutlines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Items = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    Order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseOutlines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseOutlines_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseReviews_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseReviews_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseOutlines_CourseId",
                table: "CourseOutlines",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOutlines_CourseId_Order",
                table: "CourseOutlines",
                columns: new[] { "CourseId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseReviews_CourseId",
                table: "CourseReviews",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseReviews_UserId",
                table: "CourseReviews",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Instructors_InstructorId",
                table: "Courses",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "InstructorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Instructors_InstructorId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "CourseOutlines");

            migrationBuilder.DropTable(
                name: "CourseReviews");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Certification",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LongDescription",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Sessions",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SoftwareUsed",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Students",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "WhoShouldAttend",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "WhatStudentsLearn",
                table: "Courses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Courses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CoursePictureUrl",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Courses",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<string>(
                name: "MainObjective",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Instructors_InstructorId",
                table: "Courses",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "InstructorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

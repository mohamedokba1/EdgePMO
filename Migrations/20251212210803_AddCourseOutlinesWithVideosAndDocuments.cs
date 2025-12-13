using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseOutlinesWithVideosAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseVideos_Courses_CourseId",
                table: "CourseVideos");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "CourseVideos",
                newName: "CourseOutlineId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseVideos_CourseId",
                table: "CourseVideos",
                newName: "IX_CourseVideos_CourseOutlineId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "CourseVideos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CourseVideos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "CourseVideos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CourseVideos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CourseVideos",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "CourseDocuments",
                columns: table => new
                {
                    CourseDocumentId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CourseOutlineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DocumentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseDocuemnts", x => x.CourseDocumentId);
                    table.ForeignKey(
                        name: "FK_CourseDocuemnts_CourseOutlines_CourseOutlineId",
                        column: x => x.CourseOutlineId,
                        principalTable: "CourseOutlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseVideos_CourseOutlineId_Order",
                table: "CourseVideos",
                columns: new[] { "CourseOutlineId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseDocuemnts_CourseOutlineId",
                table: "CourseDocuments",
                column: "CourseOutlineId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseDocuemnts_DocumentUrl",
                table: "CourseDocuments",
                column: "DocumentUrl",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVideos_CourseOutlines_CourseOutlineId",
                table: "CourseVideos",
                column: "CourseOutlineId",
                principalTable: "CourseOutlines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseVideos_CourseOutlines_CourseOutlineId",
                table: "CourseVideos");

            migrationBuilder.DropTable(
                name: "CourseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CourseVideos_CourseOutlineId_Order",
                table: "CourseVideos");

            migrationBuilder.RenameColumn(
                name: "CourseOutlineId",
                table: "CourseVideos",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseVideos_CourseOutlineId",
                table: "CourseVideos",
                newName: "IX_CourseVideos_CourseId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "CourseVideos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CourseVideos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "CourseVideos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CourseVideos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CourseVideos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseVideos_Courses_CourseId",
                table: "CourseVideos",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

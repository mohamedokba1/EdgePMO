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
            // Step 1: Drop the existing foreign key before renaming
            migrationBuilder.DropForeignKey(
                name: "FK_CourseVideos_Courses_CourseId",
                table: "CourseVideos");

            // Step 2: Drop the old index
            migrationBuilder.DropIndex(
                name: "IX_CourseVideos_CourseId",
                table: "CourseVideos");

            // Step 3: Delete any existing CourseVideos records (to avoid FK violations)
            migrationBuilder.Sql("DELETE FROM \"CourseVideos\";");

            // Step 4: Rename the column
            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "CourseVideos",
                newName: "CourseOutlineId");

            // Step 5: Alter columns
            migrationBuilder.AlterColumn<Guid>(
                name: "CourseVideoId",
                table: "CourseVideos",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CourseVideos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "CourseVideos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CourseVideos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
                table: "CourseVideos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            // Step 6: Create CourseDocuments table
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
                    table.PrimaryKey("PK_CourseDocuments", x => x.CourseDocumentId);
                    table.ForeignKey(
                        name: "FK_CourseDocuments_CourseOutlines_CourseOutlineId",
                        column: x => x.CourseOutlineId,
                        principalTable: "CourseOutlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Step 7: Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_CourseVideos_CourseOutlineId",
                table: "CourseVideos",
                column: "CourseOutlineId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVideos_CourseOutlineId_Order",
                table: "CourseVideos",
                columns: new[] { "CourseOutlineId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseDocuments_CourseOutlineId",
                table: "CourseDocuments",
                column: "CourseOutlineId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseDocuments_DocumentUrl",
                table: "CourseDocuments",
                column: "DocumentUrl",
                unique: true);

            // Step 8: Add foreign key constraint
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
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_CourseVideos_CourseOutlines_CourseOutlineId",
                table: "CourseVideos");

            // Drop CourseDocuments table
            migrationBuilder.DropTable(
                name: "CourseDocuments");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_CourseVideos_CourseOutlineId_Order",
                table: "CourseVideos");

            migrationBuilder.DropIndex(
                name: "IX_CourseVideos_CourseOutlineId",
                table: "CourseVideos");

            // Rename column back
            migrationBuilder.RenameColumn(
                name: "CourseOutlineId",
                table: "CourseVideos",
                newName: "CourseId");

            // Restore original column definitions
            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
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
                name: "CourseVideoId",
                table: "CourseVideos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            // Restore foreign key to Courses
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

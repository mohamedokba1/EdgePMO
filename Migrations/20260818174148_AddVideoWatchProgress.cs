using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoWatchProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoWatchProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CourseVideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WatchedSeconds = table.Column<double>(type: "double precision", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    FirstWatchedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastWatchedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoWatchProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoWatchProgresses_CourseVideos_CourseVideoId",
                        column: x => x.CourseVideoId,
                        principalTable: "CourseVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoWatchProgresses_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoWatchProgresses_CourseVideoId_UserId",
                table: "VideoWatchProgresses",
                columns: new[] { "CourseVideoId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoWatchProgresses_UserId",
                table: "VideoWatchProgresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoWatchProgresses");
        }
    }
}

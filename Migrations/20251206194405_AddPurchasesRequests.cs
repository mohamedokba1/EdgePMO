using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasesRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Courses_CourseId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Template_TemplateId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_users_UserId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTemplate_Purchase_PurchaseId",
                table: "UserTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTemplate_Template_TemplateId",
                table: "UserTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTemplate_users_UserId",
                table: "UserTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTemplate",
                table: "UserTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Template",
                table: "Template");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase");

            migrationBuilder.RenameTable(
                name: "UserTemplate",
                newName: "UserTemplates");

            migrationBuilder.RenameTable(
                name: "Template",
                newName: "Templates");

            migrationBuilder.RenameTable(
                name: "Purchase",
                newName: "Purchases");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplate_UserId_TemplateId",
                table: "UserTemplates",
                newName: "IX_UserTemplates_UserId_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplate_UserId",
                table: "UserTemplates",
                newName: "IX_UserTemplates_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplate_TemplateId",
                table: "UserTemplates",
                newName: "IX_UserTemplates_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplate_PurchaseId",
                table: "UserTemplates",
                newName: "IX_UserTemplates_PurchaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_UserId",
                table: "Purchases",
                newName: "IX_Purchases_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_TemplateId",
                table: "Purchases",
                newName: "IX_Purchases_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_Status",
                table: "Purchases",
                newName: "IX_Purchases_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_PurchaseType",
                table: "Purchases",
                newName: "IX_Purchases_PurchaseType");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_CourseId",
                table: "Purchases",
                newName: "IX_Purchases_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTemplates",
                table: "UserTemplates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Templates",
                table: "Templates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PurchaseRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_CourseId",
                table: "PurchaseRequests",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_TemplateId",
                table: "PurchaseRequests",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_UserId",
                table: "PurchaseRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Courses_CourseId",
                table: "Purchases",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Templates_TemplateId",
                table: "Purchases",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_users_UserId",
                table: "Purchases",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTemplates_Purchases_PurchaseId",
                table: "UserTemplates",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTemplates_Templates_TemplateId",
                table: "UserTemplates",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTemplates_users_UserId",
                table: "UserTemplates",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // create unique partial index so multiple nulls allowed but non-null values are unique
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_IdempotencyKey",
                table: "PurchaseRequests",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Courses_CourseId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Templates_TemplateId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_users_UserId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTemplates_Purchases_PurchaseId",
                table: "UserTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTemplates_Templates_TemplateId",
                table: "UserTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTemplates_users_UserId",
                table: "UserTemplates");

            migrationBuilder.DropTable(
                name: "PurchaseRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTemplates",
                table: "UserTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Templates",
                table: "Templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases");

            migrationBuilder.RenameTable(
                name: "UserTemplates",
                newName: "UserTemplate");

            migrationBuilder.RenameTable(
                name: "Templates",
                newName: "Template");

            migrationBuilder.RenameTable(
                name: "Purchases",
                newName: "Purchase");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplates_UserId_TemplateId",
                table: "UserTemplate",
                newName: "IX_UserTemplate_UserId_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplates_UserId",
                table: "UserTemplate",
                newName: "IX_UserTemplate_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplates_TemplateId",
                table: "UserTemplate",
                newName: "IX_UserTemplate_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTemplates_PurchaseId",
                table: "UserTemplate",
                newName: "IX_UserTemplate_PurchaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_UserId",
                table: "Purchase",
                newName: "IX_Purchase_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_TemplateId",
                table: "Purchase",
                newName: "IX_Purchase_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_Status",
                table: "Purchase",
                newName: "IX_Purchase_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_PurchaseType",
                table: "Purchase",
                newName: "IX_Purchase_PurchaseType");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_CourseId",
                table: "Purchase",
                newName: "IX_Purchase_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTemplate",
                table: "UserTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Template",
                table: "Template",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Courses_CourseId",
                table: "Purchase",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Template_TemplateId",
                table: "Purchase",
                column: "TemplateId",
                principalTable: "Template",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_users_UserId",
                table: "Purchase",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTemplate_Purchase_PurchaseId",
                table: "UserTemplate",
                column: "PurchaseId",
                principalTable: "Purchase",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTemplate_Template_TemplateId",
                table: "UserTemplate",
                column: "TemplateId",
                principalTable: "Template",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTemplate_users_UserId",
                table: "UserTemplate",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_IdempotencyKey",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "PurchaseRequests");
        }
    }
}

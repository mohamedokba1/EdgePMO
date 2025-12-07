using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "RequestedAmount",
                table: "PurchaseRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PurchaseRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RequestedAmount",
                table: "PurchaseRequests",
                type: "numeric",
                nullable: true);
        }
    }
}

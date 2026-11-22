using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceStore.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryOptionsToAdminFoodOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHomeDeliveryAvailable",
                table: "AdminFoodOrders",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUptownDeliveryAvailable",
                table: "AdminFoodOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHomeDeliveryAvailable",
                table: "AdminFoodOrders");

            migrationBuilder.DropColumn(
                name: "IsUptownDeliveryAvailable",
                table: "AdminFoodOrders");
        }
    }
}

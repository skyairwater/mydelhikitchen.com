using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceStore.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryChargesToAdminFoodOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCharge",
                table: "AdminFoodOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryCharge",
                table: "AdminFoodOrders");
        }
    }
}

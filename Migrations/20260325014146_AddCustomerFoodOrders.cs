using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcommerceStore.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFoodOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerFoodOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniqueOrderId = table.Column<string>(type: "text", nullable: false),
                    AdminFoodOrderId = table.Column<int>(type: "integer", nullable: false),
                    CustomerEmail = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MainQuantity = table.Column<int>(type: "integer", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    DeliveryCharge = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFoodOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFoodOrders_AdminFoodOrders_AdminFoodOrderId",
                        column: x => x.AdminFoodOrderId,
                        principalTable: "AdminFoodOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFoodOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerFoodOrderId = table.Column<int>(type: "integer", nullable: false),
                    AdminFoodOrderItemId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFoodOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFoodOrderItems_CustomerFoodOrders_CustomerFoodOrder~",
                        column: x => x.CustomerFoodOrderId,
                        principalTable: "CustomerFoodOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFoodOrderItems_CustomerFoodOrderId",
                table: "CustomerFoodOrderItems",
                column: "CustomerFoodOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFoodOrders_AdminFoodOrderId",
                table: "CustomerFoodOrders",
                column: "AdminFoodOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFoodOrderItems");

            migrationBuilder.DropTable(
                name: "CustomerFoodOrders");
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcommerceStore.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminFoodOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminFoodOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Menu = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    MaxOrders = table.Column<int>(type: "integer", nullable: true),
                    PhotoPaths = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminFoodOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminFoodOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminFoodOrderId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminFoodOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminFoodOrderItems_AdminFoodOrders_AdminFoodOrderId",
                        column: x => x.AdminFoodOrderId,
                        principalTable: "AdminFoodOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminFoodOrderItems_AdminFoodOrderId",
                table: "AdminFoodOrderItems",
                column: "AdminFoodOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminFoodOrderItems");

            migrationBuilder.DropTable(
                name: "AdminFoodOrders");
        }
    }
}

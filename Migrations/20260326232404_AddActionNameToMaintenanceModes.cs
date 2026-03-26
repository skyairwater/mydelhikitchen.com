using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceStore.Migrations
{
    /// <inheritdoc />
    public partial class AddActionNameToMaintenanceModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionName",
                table: "MaintenanceModes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionName",
                table: "MaintenanceModes");
        }
    }
}

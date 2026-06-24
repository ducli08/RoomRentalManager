using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWaterUnitPriceFromUtilityReading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "waterUnitPrice",
                table: "utilityReading");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "waterUnitPrice",
                table: "utilityReading",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

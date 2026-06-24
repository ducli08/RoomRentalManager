using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWaterIndexFromUtilityReading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "newWaterIndex",
                table: "utilityReading");

            migrationBuilder.DropColumn(
                name: "oldWaterIndex",
                table: "utilityReading");

            migrationBuilder.DropColumn(
                name: "waterUsage",
                table: "utilityReading");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "newWaterIndex",
                table: "utilityReading",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "oldWaterIndex",
                table: "utilityReading",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "waterUsage",
                table: "utilityReading",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

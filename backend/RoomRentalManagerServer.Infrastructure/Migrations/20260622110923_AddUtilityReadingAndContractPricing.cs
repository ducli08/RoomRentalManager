using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilityReadingAndContractPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "month",
                table: "invoice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "utilityReadingId",
                table: "invoice",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "invoice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "electricUnitPrice",
                table: "contract",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "garbageFeePerYear",
                table: "contract",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "waterUnitPrice",
                table: "contract",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "utilityReading",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    contractId = table.Column<long>(type: "bigint", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    oldElectricIndex = table.Column<decimal>(type: "numeric", nullable: false),
                    newElectricIndex = table.Column<decimal>(type: "numeric", nullable: false),
                    electricUsage = table.Column<decimal>(type: "numeric", nullable: false),
                    oldWaterIndex = table.Column<decimal>(type: "numeric", nullable: false),
                    newWaterIndex = table.Column<decimal>(type: "numeric", nullable: false),
                    waterUsage = table.Column<decimal>(type: "numeric", nullable: false),
                    electricUnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    waterUnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creatorUser = table.Column<string>(type: "text", nullable: false),
                    updaterUser = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilityReading", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_utilityReading_contractId_month_year",
                table: "utilityReading",
                columns: new[] { "contractId", "month", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "utilityReading");

            migrationBuilder.DropColumn(
                name: "month",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "utilityReadingId",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "year",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "electricUnitPrice",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "garbageFeePerYear",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "waterUnitPrice",
                table: "contract");
        }
    }
}

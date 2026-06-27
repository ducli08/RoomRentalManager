using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGarbageFeeToMonthlyPerPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "garbageFeePerYear",
                table: "contract",
                newName: "garbageFeePerMonthPerPerson");

            migrationBuilder.Sql(
                @"UPDATE contract SET ""garbageFeePerMonthPerPerson"" = ""garbageFeePerMonthPerPerson"" / 12;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE contract SET ""garbageFeePerMonthPerPerson"" = ""garbageFeePerMonthPerPerson"" * 12;");

            migrationBuilder.RenameColumn(
                name: "garbageFeePerMonthPerPerson",
                table: "contract",
                newName: "garbageFeePerYear");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    public partial class AddContractTenantIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long[]>(
                name: "tenantIds",
                table: "contract",
                type: "bigint[]",
                nullable: false,
                defaultValueSql: "'{}'::bigint[]");

            migrationBuilder.Sql("""
                UPDATE contract
                SET "tenantIds" = ARRAY["tenantId"]
                WHERE "tenantId" > 0;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tenantIds",
                table: "contract");
        }
    }
}

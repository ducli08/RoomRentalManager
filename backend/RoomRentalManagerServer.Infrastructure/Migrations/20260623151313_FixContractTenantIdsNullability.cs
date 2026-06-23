using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixContractTenantIdsNullability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE contract
                ADD COLUMN IF NOT EXISTS "tenantIds" bigint[];

                ALTER TABLE contract
                ALTER COLUMN "tenantIds" SET DEFAULT '{}'::bigint[];

                UPDATE contract
                SET "tenantIds" = ARRAY["tenantId"]
                WHERE ("tenantIds" IS NULL OR cardinality("tenantIds") = 0)
                  AND "tenantId" > 0;

                UPDATE contract
                SET "tenantIds" = '{}'::bigint[]
                WHERE "tenantIds" IS NULL;

                ALTER TABLE contract
                ALTER COLUMN "tenantIds" SET NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long[]>(
                name: "tenantIds",
                table: "contract",
                type: "bigint[]",
                nullable: true,
                defaultValueSql: "'{}'::bigint[]",
                oldClrType: typeof(long[]),
                oldType: "bigint[]",
                oldDefaultValueSql: "'{}'::bigint[]");
        }
    }
}

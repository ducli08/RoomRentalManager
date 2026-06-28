using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InvoicePaymentFlowRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_submission");

            migrationBuilder.DropColumn(
                name: "issuedAt",
                table: "invoice");

            migrationBuilder.RenameColumn(
                name: "AmountPaid",
                table: "payment",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "amountPaid",
                table: "invoice",
                newName: "paidAmount");

            migrationBuilder.AlterColumn<DateTime>(
                name: "paymentDate",
                table: "payment",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelledAt",
                table: "payment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancelledReason",
                table: "payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "evidencePublicId",
                table: "payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "evidenceUrl",
                table: "payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "referenceCode",
                table: "payment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "rejectedReason",
                table: "payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "payment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE payment
                SET status = 3,
                    "referenceCode" = 'LEGACY-P' || id
                WHERE status = 0;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "year",
                table: "invoice",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "utilityReadingId",
                table: "invoice",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "month",
                table: "invoice",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoiceCode",
                table: "invoice",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "occupancyDaysSnapshot",
                table: "invoice",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "periodEndSnapshot",
                table: "invoice",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "periodStartSnapshot",
                table: "invoice",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "roomNameSnapshot",
                table: "invoice",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tenantNameSnapshot",
                table: "invoice",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "invoice_item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoiceId = table.Column<long>(type: "bigint", nullable: false),
                    itemType = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    unitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    sortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_item", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_invoiceId",
                table: "payment",
                column: "invoiceId",
                unique: true,
                filter: "\"status\" IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_payment_referenceCode",
                table: "payment",
                column: "referenceCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_item");

            migrationBuilder.DropIndex(
                name: "IX_payment_invoiceId",
                table: "payment");

            migrationBuilder.DropIndex(
                name: "IX_payment_referenceCode",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "cancelledAt",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "cancelledReason",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "evidencePublicId",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "evidenceUrl",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "referenceCode",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "rejectedReason",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "status",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "invoiceCode",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "occupancyDaysSnapshot",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "periodEndSnapshot",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "periodStartSnapshot",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "roomNameSnapshot",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "tenantNameSnapshot",
                table: "invoice");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "payment",
                newName: "AmountPaid");

            migrationBuilder.RenameColumn(
                name: "paidAmount",
                table: "invoice",
                newName: "amountPaid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "paymentDate",
                table: "payment",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "year",
                table: "invoice",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "utilityReadingId",
                table: "invoice",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "month",
                table: "invoice",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "issuedAt",
                table: "invoice",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment_submission",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creatorUser = table.Column<string>(type: "text", nullable: false),
                    declaredAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    evidencePublicId = table.Column<string>(type: "text", nullable: false),
                    evidenceUrl = table.Column<string>(type: "text", nullable: false),
                    invoiceId = table.Column<long>(type: "bigint", nullable: false),
                    lastUpdateUser = table.Column<string>(type: "text", nullable: false),
                    rejectedReason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_submission", x => x.id);
                });
        }
    }
}

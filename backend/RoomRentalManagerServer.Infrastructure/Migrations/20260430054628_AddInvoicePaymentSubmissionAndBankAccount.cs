using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicePaymentSubmissionAndBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Breaking change (v1): Payment schema had wrong column types. To avoid risky casts during migration,
            // we drop & recreate the table with the new, correct schema.
            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.AddColumn<decimal>(
                name: "amountPaid",
                table: "invoice",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelledAt",
                table: "invoice",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "issuedAt",
                table: "invoice",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoiceId = table.Column<long>(type: "bigint", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    paymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paymentMethod = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creatorUser = table.Column<string>(type: "text", nullable: false),
                    lastUpdateUser = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bank_account",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bankCode = table.Column<string>(type: "text", nullable: false),
                    accountNumber = table.Column<string>(type: "text", nullable: false),
                    accountName = table.Column<string>(type: "text", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creatorUser = table.Column<string>(type: "text", nullable: false),
                    lastUpdateUser = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_submission",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoiceId = table.Column<long>(type: "bigint", nullable: false),
                    declaredAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    evidenceUrl = table.Column<string>(type: "text", nullable: false),
                    evidencePublicId = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rejectedReason = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creatorUser = table.Column<string>(type: "text", nullable: false),
                    lastUpdateUser = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_submission", x => x.id);
                });

            // Map legacy invoiceStatus values to the new enum:
            // Legacy: Unpaid=1, Paid=2, Overdue=3
            // New: Draft=1, Issued=2, PartiallyPaid=3, Paid=4, Cancelled=5
            //
            // We treat legacy Unpaid/Overdue as Issued (2) and legacy Paid as Paid (4).
            // For Paid invoices, set amountPaid = totalAmount.
            migrationBuilder.Sql(@"
UPDATE invoice
SET ""invoiceStatus"" = CASE
  WHEN ""invoiceStatus"" = 2 THEN 4
  WHEN ""invoiceStatus"" IN (1, 3) THEN 2
  ELSE ""invoiceStatus""
END,
""amountPaid"" = CASE
  WHEN ""invoiceStatus"" = 2 THEN ""totalAmount""
  ELSE ""amountPaid""
END
;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_account");

            migrationBuilder.DropTable(
                name: "payment_submission");

            migrationBuilder.DropColumn(
                name: "amountPaid",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "cancelledAt",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "issuedAt",
                table: "invoice");

            migrationBuilder.DropTable(
                name: "payment");

            // Recreate legacy Payment table schema (as it existed before this migration)
            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoiceId = table.Column<long>(type: "bigint", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    paymentDate = table.Column<decimal>(type: "numeric", nullable: false),
                    paymentMethod = table.Column<int>(type: "integer", nullable: false),
                    statusPayment = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lastUpdateUser = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.id);
                });
        }
    }
}

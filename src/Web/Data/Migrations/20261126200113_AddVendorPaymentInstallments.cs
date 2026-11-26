using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlanner.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorPaymentInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalancePaid",
                table: "VendorAssignment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE "VendorAssignment"
                SET "OpeningBalancePaid" = CASE
                    WHEN "TotalPaid" > 0 THEN "TotalPaid"
                    ELSE "DepositPaid"
                END;
                """);

            migrationBuilder.DropColumn(
                name: "DepositPaid",
                table: "VendorAssignment");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "VendorAssignment");

            migrationBuilder.DropColumn(
                name: "TotalPaid",
                table: "VendorAssignment");

            migrationBuilder.CreateTable(
                name: "VendorPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorPayments_VendorAssignment_VendorAssignmentId",
                        column: x => x.VendorAssignmentId,
                        principalTable: "VendorAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorPayments_VendorAssignmentId",
                table: "VendorPayments",
                column: "VendorAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositPaid",
                table: "VendorAssignment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaid",
                table: "VendorAssignment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "VendorAssignment",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE "VendorAssignment" AS assignment
                SET "DepositPaid" = assignment."OpeningBalancePaid",
                    "TotalPaid" = assignment."OpeningBalancePaid" + COALESCE((
                        SELECT SUM(payment."Amount")
                        FROM "VendorPayments" AS payment
                        WHERE payment."VendorAssignmentId" = assignment."Id"
                            AND payment."PaymentDate" IS NOT NULL
                    ), 0),
                    "PaymentStatus" = CASE
                        WHEN assignment."PaymentAmount" > 0
                            AND assignment."OpeningBalancePaid" + COALESCE((
                                SELECT SUM(payment."Amount")
                                FROM "VendorPayments" AS payment
                                WHERE payment."VendorAssignmentId" = assignment."Id"
                                    AND payment."PaymentDate" IS NOT NULL
                            ), 0) >= assignment."PaymentAmount"
                        THEN 2
                        ELSE 1
                    END;
                """);

            migrationBuilder.DropTable(
                name: "VendorPayments");

            migrationBuilder.DropColumn(
                name: "OpeningBalancePaid",
                table: "VendorAssignment");
        }
    }
}

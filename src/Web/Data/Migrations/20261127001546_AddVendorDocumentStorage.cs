using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlanner.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorDocumentStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorDocument_Vendors_VendorId",
                table: "VendorDocument");

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "VendorDocument",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "VendorDocument",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeletionPending",
                table: "VendorDocument",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "VendorDocument",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "VendorDocument",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "VendorDocument",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "VendorDocument",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_VendorDocument_Storage",
                table: "VendorDocument",
                sql: "(\"FileUrl\" IS NOT NULL AND \"StorageKey\" IS NULL) OR (\"FileUrl\" IS NULL AND \"StorageKey\" IS NOT NULL AND \"VendorAssignmentId\" IS NOT NULL AND \"FileName\" IS NOT NULL AND \"ContentType\" IS NOT NULL AND \"FileSize\" IS NOT NULL AND \"FileSize\" > 0)");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorDocument_Vendors_VendorId",
                table: "VendorDocument",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("No se puede revertir sin perder referencias a archivos almacenados.");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlanner.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeddingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WeddingTableId",
                table: "Guests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WeddingTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeddingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeddingTables", x => x.Id);
                    table.UniqueConstraint("AK_WeddingTables_Id_WeddingId", x => new { x.Id, x.WeddingId });
                    table.CheckConstraint("CK_WeddingTables_Capacity", "\"Capacity\" > 0");
                    table.ForeignKey(
                        name: "FK_WeddingTables_Weddings_WeddingId",
                        column: x => x.WeddingId,
                        principalTable: "Weddings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "WeddingTables" ("Id", "WeddingId", "Name", "Capacity", "CreatedAt")
                SELECT md5("WeddingId"::text || ':' || "TableNumber"::text)::uuid,
                    "WeddingId", "TableNumber"::text,
                    GREATEST(1, SUM(
                        CASE WHEN "RSVPStatus" = 2 THEN
                            1 + CASE WHEN "CanBringCompanion" AND "CompanionStatus" = 1 THEN 1 ELSE 0 END
                        ELSE 0 END
                    )::integer),
                    CURRENT_TIMESTAMP AT TIME ZONE 'UTC'
                FROM "Guests"
                WHERE "TableNumber" IS NOT NULL
                GROUP BY "WeddingId", "TableNumber";

                UPDATE "Guests"
                SET "WeddingTableId" = md5("WeddingId"::text || ':' || "TableNumber"::text)::uuid
                WHERE "TableNumber" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "TableNumber",
                table: "Guests");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_WeddingTableId_WeddingId",
                table: "Guests",
                columns: new[] { "WeddingTableId", "WeddingId" });

            migrationBuilder.CreateIndex(
                name: "IX_WeddingTables_WeddingId_Name",
                table: "WeddingTables",
                columns: new[] { "WeddingId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Guests_WeddingTables_WeddingTableId_WeddingId",
                table: "Guests",
                columns: new[] { "WeddingTableId", "WeddingId" },
                principalTable: "WeddingTables",
                principalColumns: new[] { "Id", "WeddingId" },
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "No se puede revertir esta migración sin perder mesas y asignaciones; restaura una copia de seguridad.");
        }
    }
}

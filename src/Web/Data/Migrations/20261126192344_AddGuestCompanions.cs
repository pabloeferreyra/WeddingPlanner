using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlanner.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestCompanions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanBringCompanion",
                table: "Guests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CompanionDietaryRestrictions",
                table: "Guests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanionMealPreference",
                table: "Guests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanionName",
                table: "Guests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanionNotes",
                table: "Guests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanionStatus",
                table: "Guests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanBringCompanion",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CompanionDietaryRestrictions",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CompanionMealPreference",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CompanionName",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CompanionNotes",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CompanionStatus",
                table: "Guests");
        }
    }
}

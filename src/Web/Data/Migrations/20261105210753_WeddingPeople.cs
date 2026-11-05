using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlanner.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class WeddingPeople : Migration
    {
        private static readonly string[] Columns = ["Name", "Email", "Phone"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var c in Columns)
            {
                migrationBuilder.RenameColumn(name: "Bride" + c, table: "Weddings", newName: "Bride_" + c);
                migrationBuilder.RenameColumn(name: "Groom" + c, table: "Weddings", newName: "Groom_" + c);
                migrationBuilder.AddColumn<string>(name: "Celebrated_" + c, table: "Weddings", type: "text", nullable: true);
            }

            migrationBuilder.AlterColumn<string>(name: "Bride_Name", table: "Weddings", type: "text", nullable: true, oldClrType: typeof(string), oldType: "text");
            migrationBuilder.AlterColumn<string>(name: "Groom_Name", table: "Weddings", type: "text", nullable: true, oldClrType: typeof(string), oldType: "text");

            migrationBuilder.AddColumn<bool>(name: "IsWedding", table: "Weddings", type: "boolean", nullable: false, defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsWedding", table: "Weddings");

            migrationBuilder.Sql("UPDATE \"Weddings\" SET \"Bride_Name\" = COALESCE(\"Bride_Name\", ''), \"Groom_Name\" = COALESCE(\"Groom_Name\", '')");
            migrationBuilder.AlterColumn<string>(name: "Bride_Name", table: "Weddings", type: "text", nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Groom_Name", table: "Weddings", type: "text", nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);

            foreach (var c in Columns)
            {
                migrationBuilder.DropColumn(name: "Celebrated_" + c, table: "Weddings");
                migrationBuilder.RenameColumn(name: "Bride_" + c, table: "Weddings", newName: "Bride" + c);
                migrationBuilder.RenameColumn(name: "Groom_" + c, table: "Weddings", newName: "Groom" + c);
            }
        }
    }
}

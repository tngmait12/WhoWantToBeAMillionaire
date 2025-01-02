using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhoWantToBeAMillionaire.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateAccess",
                table: "Statisticals",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CountAccess",
                table: "Statisticals",
                newName: "PlayerRounds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlayerRounds",
                table: "Statisticals",
                newName: "CountAccess");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Statisticals",
                newName: "DateAccess");
        }
    }
}

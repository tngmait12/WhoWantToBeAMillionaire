using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhoWantToBeAMillionaire.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HostPlayerId",
                table: "Rooms",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Players",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HostPlayerId",
                table: "Rooms",
                column: "HostPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_User_HostPlayerId",
                table: "Rooms",
                column: "HostPlayerId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_User_HostPlayerId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_HostPlayerId",
                table: "Rooms");

            migrationBuilder.AlterColumn<string>(
                name: "HostPlayerId",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "Players",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}

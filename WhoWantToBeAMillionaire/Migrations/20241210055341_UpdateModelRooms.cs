using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhoWantToBeAMillionaire.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_RoomId",
                table: "Questions",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Rooms_RoomId",
                table: "Questions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Rooms_RoomId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_RoomId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Questions");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UserId",
                table: "UploadedFiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayBackLogs_UserId",
                table: "PlayBackLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayBackLogs_Users_UserId",
                table: "PlayBackLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedFiles_Users_UserId",
                table: "UploadedFiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayBackLogs_Users_UserId",
                table: "PlayBackLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_Users_UserId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_UserId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_PlayBackLogs_UserId",
                table: "PlayBackLogs");
        }
    }
}

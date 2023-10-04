using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainLibraryID",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MainLibraryID",
                table: "AspNetUsers",
                column: "MainLibraryID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Libraries_MainLibraryID",
                table: "AspNetUsers",
                column: "MainLibraryID",
                principalTable: "Libraries",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Libraries_MainLibraryID",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MainLibraryID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MainLibraryID",
                table: "AspNetUsers");
        }
    }
}

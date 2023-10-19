using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLocationAndContactOfLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Libraries",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "Contact",
                table: "Libraries",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Libraries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Libraries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Libraries");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Libraries",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Libraries",
                newName: "Contact");
        }
    }
}

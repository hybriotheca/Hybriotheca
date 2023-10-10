using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyAvailableStockToBookStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "BooksInStock",
                newName: "TotalStock");

            migrationBuilder.AddColumn<int>(
                name: "AvailableStock",
                table: "BooksInStock",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableStock",
                table: "BooksInStock");

            migrationBuilder.RenameColumn(
                name: "TotalStock",
                table: "BooksInStock",
                newName: "Quantity");
        }
    }
}

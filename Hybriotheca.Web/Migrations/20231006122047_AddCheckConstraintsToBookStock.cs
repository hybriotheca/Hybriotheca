using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraintsToBookStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AvailableStock_GreaterOrEqualZero",
                table: "BooksInStock",
                sql: "[AvailableStock] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TotalStock_GreaterOrEqual_AvailableStock",
                table: "BooksInStock",
                sql: "[TotalStock] >= [AvailableStock]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AvailableStock_GreaterOrEqualZero",
                table: "BooksInStock");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TotalStock_GreaterOrEqual_AvailableStock",
                table: "BooksInStock");
        }
    }
}

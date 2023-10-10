using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemovePropertyIsPastDueDateFromLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPastDueDate",
                table: "Loans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPastDueDate",
                table: "Loans",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

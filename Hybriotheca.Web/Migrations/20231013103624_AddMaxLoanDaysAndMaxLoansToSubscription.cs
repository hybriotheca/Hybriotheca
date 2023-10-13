using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxLoanDaysAndMaxLoansToSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxLoanDays",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxLoans",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MaxLoanDays_GreaterOrEqualZero",
                table: "Subscriptions",
                sql: "[MaxLoanDays] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MaxLoans_GreaterOrEqualZero",
                table: "Subscriptions",
                sql: "[MaxLoans] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MaxLoanDays_GreaterOrEqualZero",
                table: "Subscriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MaxLoans_GreaterOrEqualZero",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "MaxLoanDays",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "MaxLoans",
                table: "Subscriptions");
        }
    }
}

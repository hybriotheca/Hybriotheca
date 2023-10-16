using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Loans",
                newName: "TermLimitDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Loans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Loans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Loans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "TermLimitDate",
                table: "Loans",
                newName: "EndDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "Loans",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

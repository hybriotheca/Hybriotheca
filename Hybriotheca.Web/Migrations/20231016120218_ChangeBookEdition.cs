using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hybriotheca.Web.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBookEdition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EbookID",
                table: "BookEditions");

            migrationBuilder.DropColumn(
                name: "IsAvailableOnline",
                table: "BookEditions");

            migrationBuilder.AddColumn<Guid>(
                name: "ePubID",
                table: "BookEditions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ePubID",
                table: "BookEditions");

            migrationBuilder.AddColumn<Guid>(
                name: "EbookID",
                table: "BookEditions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableOnline",
                table: "BookEditions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

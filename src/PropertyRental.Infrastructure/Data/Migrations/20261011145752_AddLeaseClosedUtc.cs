using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyRental.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaseClosedUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedUtc",
                schema: "dbo",
                table: "Leases",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedUtc",
                schema: "dbo",
                table: "Leases");
        }
    }
}

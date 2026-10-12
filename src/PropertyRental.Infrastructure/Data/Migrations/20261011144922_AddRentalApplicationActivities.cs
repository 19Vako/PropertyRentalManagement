using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyRental.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalApplicationActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RentalApplicationActivities",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalApplicationActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalApplicationActivities_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalApplicationActivities_RentalApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "dbo",
                        principalTable: "RentalApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalApplicationActivities_ActorUserId",
                schema: "dbo",
                table: "RentalApplicationActivities",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalApplicationActivities_ApplicationId_CreatedUtc",
                schema: "dbo",
                table: "RentalApplicationActivities",
                columns: new[] { "ApplicationId", "CreatedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalApplicationActivities",
                schema: "dbo");
        }
    }
}

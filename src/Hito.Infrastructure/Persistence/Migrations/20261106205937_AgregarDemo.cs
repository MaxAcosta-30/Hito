using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hito.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DemoExpiraEn",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DemoExpiraEn",
                table: "AspNetUsers",
                column: "DemoExpiraEn",
                filter: "\"DemoExpiraEn\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DemoExpiraEn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DemoExpiraEn",
                table: "AspNetUsers");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hito.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AprobadoEn",
                table: "Hitos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComentarioCambios",
                table: "Hitos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EntregadoEn",
                table: "Hitos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenciaUrl",
                table: "Hitos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PagadoEn",
                table: "Hitos",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AprobadoEn",
                table: "Hitos");

            migrationBuilder.DropColumn(
                name: "ComentarioCambios",
                table: "Hitos");

            migrationBuilder.DropColumn(
                name: "EntregadoEn",
                table: "Hitos");

            migrationBuilder.DropColumn(
                name: "EvidenciaUrl",
                table: "Hitos");

            migrationBuilder.DropColumn(
                name: "PagadoEn",
                table: "Hitos");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hito.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CascadaDeHitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Hitos_HitoId",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminosDeHito_Hitos_HitoId",
                table: "TerminosDeHito");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Hitos_HitoId",
                table: "Pagos",
                column: "HitoId",
                principalTable: "Hitos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminosDeHito_Hitos_HitoId",
                table: "TerminosDeHito",
                column: "HitoId",
                principalTable: "Hitos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Hitos_HitoId",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminosDeHito_Hitos_HitoId",
                table: "TerminosDeHito");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Hitos_HitoId",
                table: "Pagos",
                column: "HitoId",
                principalTable: "Hitos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminosDeHito_Hitos_HitoId",
                table: "TerminosDeHito",
                column: "HitoId",
                principalTable: "Hitos",
                principalColumn: "Id");
        }
    }
}

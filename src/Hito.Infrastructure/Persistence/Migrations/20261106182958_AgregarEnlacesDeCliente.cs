using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hito.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEnlacesDeCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnlacesDeCliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    HashDelToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiraEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevocadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnlacesDeCliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnlacesDeCliente_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnlacesDeCliente_HashDelToken",
                table: "EnlacesDeCliente",
                column: "HashDelToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnlacesDeCliente_ProyectoId",
                table: "EnlacesDeCliente",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnlacesDeCliente");
        }
    }
}

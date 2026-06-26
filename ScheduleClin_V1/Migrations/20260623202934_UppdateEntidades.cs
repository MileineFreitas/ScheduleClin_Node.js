using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleClin.Migrations
{
    /// <inheritdoc />
    public partial class UppdateEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_AspNetUsers_CriadoPorId",
                table: "Calendars");

            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_AspNetUsers_PacienteId",
                table: "Calendars");

            migrationBuilder.DropIndex(
                name: "IX_Calendars_CriadoPorId",
                table: "Calendars");

            migrationBuilder.DropIndex(
                name: "IX_Calendars_PacienteId",
                table: "Calendars");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Profiles",
                newName: "ProfileId");

            migrationBuilder.AlterColumn<int>(
                name: "CRP",
                table: "Profiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileId",
                table: "Profiles",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "CRP",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_CriadoPorId",
                table: "Calendars",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_PacienteId",
                table: "Calendars",
                column: "PacienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_AspNetUsers_CriadoPorId",
                table: "Calendars",
                column: "CriadoPorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_AspNetUsers_PacienteId",
                table: "Calendars",
                column: "PacienteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

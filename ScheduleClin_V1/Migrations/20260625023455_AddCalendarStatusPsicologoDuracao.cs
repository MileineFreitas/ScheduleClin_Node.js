using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleClin.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarStatusPsicologoDuracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Calendars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PsicologoId",
                table: "Calendars",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Calendars",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "PsicologoId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Calendars");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleClin.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCrp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Crp",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Crp",
                table: "AspNetUsers");
        }
    }
}

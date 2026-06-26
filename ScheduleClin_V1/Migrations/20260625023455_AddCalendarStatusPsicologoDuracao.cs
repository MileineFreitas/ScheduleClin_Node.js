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
            // PsicologoId já existe no banco (criado por uma migration aplicada fora deste
            // repositório) — só falta DurationMinutes, e Status precisa virar nvarchar.
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Calendars",
                type: "int",
                nullable: false,
                defaultValue: 60);

            // remove a default constraint antiga (int) antes de mudar o tipo da coluna
            migrationBuilder.Sql(@"
DECLARE @constraintName nvarchar(200)
SELECT @constraintName = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('Calendars') AND c.name = 'Status'
IF @constraintName IS NOT NULL
    EXEC('ALTER TABLE [Calendars] DROP CONSTRAINT [' + @constraintName + ']')");

            migrationBuilder.Sql("ALTER TABLE [Calendars] ALTER COLUMN [Status] nvarchar(30) NOT NULL");
            migrationBuilder.Sql("UPDATE [Calendars] SET [Status] = 'Confirmada' WHERE [Status] NOT IN ('Pendente', 'Confirmada', 'Reagendamento Solicitado', 'Cancelada')");
            migrationBuilder.Sql("ALTER TABLE [Calendars] ADD CONSTRAINT [DF_Calendars_Status] DEFAULT 'Confirmada' FOR [Status]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Calendars");

            migrationBuilder.Sql("ALTER TABLE [Calendars] DROP CONSTRAINT [DF_Calendars_Status]");
            migrationBuilder.Sql("UPDATE [Calendars] SET [Status] = '0'");
            migrationBuilder.Sql("ALTER TABLE [Calendars] ALTER COLUMN [Status] int NOT NULL");
        }
    }
}

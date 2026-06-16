using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ScheduleClin.Models;

[Table("Calendars")]
public class Calendar
{
    [Key]
    public Guid CalendarID { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    public DateTime ScheduleDate { get; set; }

    // Paciente da consulta (FK explícita + navegação — sem o "UserIdId" estranho de antes)
    public Guid? PacienteId { get; set; }
    public User? Paciente { get; set; }

    // Quem agendou (secretária, psicólogo, etc.)
    [JsonIgnore]
    public Guid? CriadoPorId { get; set; }
    [JsonIgnore]
    public User? CriadoPor { get; set; }
}

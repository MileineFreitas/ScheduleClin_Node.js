using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ScheduleClin.Models;

[Table("Calendars")]
public class Calendar
{
    [Key]
    [JsonIgnore]
    public Guid CalendarID { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    public DateTime ScheduleDate { get; set; }

    public int DurationMinutes { get; set; } = 60;

    // null = bloqueio/evento da clínica (sem paciente associado)
    public Guid? PacienteId { get; set; }

    public Guid? PsicologoId { get; set; }

    // Pendente | Confirmada | Reagendamento Solicitado | Cancelada | Finalizado
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = AppointmentStatus.Confirmada;

    // Quem agendou (gestor/admin logado)
    [JsonIgnore]
    public Guid? CriadoPorId { get; set; }
}

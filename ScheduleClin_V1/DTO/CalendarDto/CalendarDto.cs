namespace ScheduleClin.DTO;

public class CalendarDto
{
    public Guid CalendarId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? PacienteId { get; set; }
    public string? PacienteNome { get; set; }
    public Guid? PsicologoId { get; set; }
    public string? PsicologoNome { get; set; }
}

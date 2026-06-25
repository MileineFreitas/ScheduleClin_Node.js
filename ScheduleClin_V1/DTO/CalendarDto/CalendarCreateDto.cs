namespace ScheduleClin.DTO;

public class CalendarCreateDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public Guid? PacienteId { get; set; }
    public Guid? PsicologoId { get; set; }
}

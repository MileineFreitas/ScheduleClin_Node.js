namespace ScheduleClin.DTO;

public class CalendarEditDto
{
    public string? Title { get; set; }
    public DateTime? ScheduleDate { get; set; }
    public int? DurationMinutes { get; set; }
    public Guid? PsicologoId { get; set; }
    public string? Status { get; set; }
}

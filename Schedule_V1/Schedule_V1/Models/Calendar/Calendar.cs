using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Schedule_V1.Models;

[Table("Calendars")]
public class Calendar
{
    [Key]
    public Guid CalendarID { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    public User? UserId { get; set; }

    public DateTime ScheduleDate { get; set; }

    [JsonIgnore]
    public User? UserCreator { get; set; }
}
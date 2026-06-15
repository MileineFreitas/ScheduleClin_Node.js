namespace ScheduleClin.Models;

public class Profile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CRP { get; set; }
}
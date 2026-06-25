namespace ScheduleClin.DTO;

public class ProfileDto
{
    public Guid ProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

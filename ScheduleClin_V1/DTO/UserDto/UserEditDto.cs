namespace ScheduleClin.DTO;

public class UserEditDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? CPF { get; set; }
    public Guid? PerfilId { get; set; }
    public DateTime? DataNascimento { get; set; }
}

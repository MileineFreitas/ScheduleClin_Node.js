namespace ScheduleClin.DTO;


public class UserDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? CPF { get; set; }
    public Guid? PerfilId { get; set; }
    public string? Role { get; set; }
    public string? PerfilNome { get; set; }
    public string? Crp { get; set; }
    public DateTime DataNascimento { get; set; }
    public bool MustChangePassword { get; set; }
    public bool Active { get; set; }
}
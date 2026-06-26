namespace ScheduleClin.DTO;

public class UserCreateDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string CPF { get; set; } = null!;
    public Guid PerfilId { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? Crp { get; set; }            // obrigatório quando perfil = Psicologo
}

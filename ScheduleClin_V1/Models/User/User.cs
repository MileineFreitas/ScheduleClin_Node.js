using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ScheduleClin.Models;

[Table("Users")]
public class User : IdentityUser<Guid>
{

    [PersonalData]                 // marca dado pessoal (LGPD) p/ ferramentas do Identity
    [StringLength(14)]
    public string? CPF { get; set; }
    public Guid? PerfilId { get; set; }

    public DateTime DataNascimento { get; set; }
    public DateTimeOffset CreateAt { get; set; } = DateTime.UtcNow;

    // RF02 — troca obrigatória de senha no primeiro acesso (senha provisória)
    public bool MustChangePassword { get; set; } = true;

    public bool Active { get; set; } = true;
}

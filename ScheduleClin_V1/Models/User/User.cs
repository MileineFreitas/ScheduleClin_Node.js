using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ScheduleClin.Models;

[Table("Users")]
public class User : IdentityUser<Guid>
{
    // UserName, Email, PasswordHash, SecurityStamp, Lockout etc. vêm de IdentityUser.
    // O Identity guarda a senha SEMPRE como hash.

    [PersonalData]                 // marca dado pessoal (LGPD) p/ ferramentas do Identity
    [StringLength(14)]
    public string? CPF { get; set; }

    // Dados profissionais (ex.: CRP do psicólogo). Opcional: nem todo perfil tem.
    public Guid? PerfilId { get; set; }
    public Profile? Perfil { get; set; }

    public DateTime DataNascimento { get; set; }
    public DateTimeOffset CreateAt { get; set; } = DateTime.UtcNow;

    // RF02 — troca obrigatória de senha no primeiro acesso (senha provisória)
    public bool MustChangePassword { get; set; } = true;

    // RF07 — inativação de usuário pelo administrador (em vez de excluir)
    public bool Active { get; set; } = true;
}

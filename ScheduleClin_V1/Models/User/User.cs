using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ScheduleClin.Models;

[Table("Users")]
public class User
{
    [Required]
    [JsonIgnore]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
    [StringLength(150)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public double? CPF { get; set; } //CPF será opcional, pois nem todos os usuários podem ter um CPF válido

    [Required]
    public Profile? Perfil { get; set; }

    public DateTime DataNascimento { get; set; }
    public DateTimeOffset CreateAt { get; set; } = DateTime.UtcNow;
}
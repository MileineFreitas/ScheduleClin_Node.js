using System.ComponentModel.DataAnnotations;

namespace ScheduleClin.Models;

// RNF08 — registro de auditoria das ações sensíveis do sistema
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime DateUtc { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }

    [StringLength(256)]
    public string? UserName { get; set; }

    // Ex.: "Login", "LoginFalhou", "Logout", "Added Calendar", "Modified User"
    [StringLength(80)]
    public string Action { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Entity { get; set; }

    [StringLength(80)]
    public string? EntityId { get; set; }

    [StringLength(1000)]
    public string? Details { get; set; }

    [StringLength(64)]
    public string? IpAddress { get; set; }
}

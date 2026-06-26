using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ScheduleClin.Models;

namespace ScheduleClin.Data;

// RNF08 — audita automaticamente alterações em entidades sensíveis (User, Calendar),
// gravando o registro na MESMA transação do SaveChanges.
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _http;

    // Propriedades que nunca podem aparecer no log (segredos) nem contar como alteração relevante
    private static readonly HashSet<string> Ruido = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
        "AccessFailedCount", "LockoutEnd", "LockoutEnabled",
        "NormalizedUserName", "NormalizedEmail"
    };

    // Entidades auditadas
    private static readonly HashSet<string> Auditadas = new() { nameof(User), nameof(Calendar) };

    public AuditSaveChangesInterceptor(IHttpContextAccessor http) => _http = http;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is not null) Auditar(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null) Auditar(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void Auditar(DbContext context)
    {
        var http = _http.HttpContext;
        if (http is null) return; // sem requisição (ex.: seeding no startup) -> não audita

        var userIdStr = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName  = http.User.Identity?.Name;
        var ip        = http.Connection.RemoteIpAddress?.ToString();

        var logs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog) continue;                       // evita auditar o próprio log
            var typeName = entry.Entity.GetType().Name;
            if (!Auditadas.Contains(typeName)) continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            // Em alterações, ignora as que mudaram apenas campos de "ruído" (ex.: login housekeeping)
            string? details = null;
            if (entry.State == EntityState.Modified)
            {
                details = MontarDetalhes(entry);
                if (details is null) continue; // nada relevante mudou
            }

            var entityId = entry.Metadata.FindPrimaryKey()?.Properties
                .Select(p => entry.Property(p.Name).CurrentValue?.ToString())
                .FirstOrDefault();

            logs.Add(new AuditLog
            {
                UserId    = Guid.TryParse(userIdStr, out var g) ? g : null,
                UserName  = userName,
                Action    = $"{entry.State} {typeName}",
                Entity    = typeName,
                EntityId  = entityId,
                Details   = details,
                IpAddress = ip
            });
        }

        if (logs.Count > 0)
            context.Set<AuditLog>().AddRange(logs);
    }

    // Lista os campos relevantes que mudaram. Retorna null se só mudou "ruído".
    private static string? MontarDetalhes(EntityEntry entry)
    {
        var sb = new StringBuilder();
        foreach (var p in entry.Properties)
        {
            if (!p.IsModified) continue;
            if (Ruido.Contains(p.Metadata.Name)) continue;
            sb.Append(p.Metadata.Name).Append('=').Append(p.CurrentValue).Append("; ");
        }
        return sb.Length > 0 ? sb.ToString().Trim() : null;
    }
}

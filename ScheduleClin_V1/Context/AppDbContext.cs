using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScheduleClin.Models;

namespace ScheduleClin.Context;

// Herda de IdentityDbContext para que o Identity gerencie usuários, papéis e relações
public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }   // RNF08 — auditoria

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // configura o esquema do Identity
    }
}

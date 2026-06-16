using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScheduleClin.Models;

namespace ScheduleClin.Context;

// Antes herdava de DbContext. Agora herda de IdentityDbContext para que o
// Identity gerencie as tabelas de usuários, papéis e relações
public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<Profile> Profiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); //configura o esquema do Identity
    }
}

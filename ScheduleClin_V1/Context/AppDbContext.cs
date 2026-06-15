using Microsoft.EntityFrameworkCore;
using ScheduleClin.Models;

namespace ScheduleClin.Context;

public class AppDbContext : DbContext

{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Calendar> Calendars { get; set; }
}
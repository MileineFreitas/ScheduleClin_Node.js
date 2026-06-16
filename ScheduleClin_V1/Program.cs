using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScheduleClin.Context;
using ScheduleClin.Data;
using ScheduleClin.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// dotnet user-secrets para mais segurança
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ───────── ASP.NET Core Identity ─────────
builder.Services
    .AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        // Política de senha (RNF03 — senha forte; o hash é feito pelo próprio Identity)
        options.Password.RequiredLength         = 8;
        options.Password.RequireUppercase       = true;
        options.Password.RequireLowercase       = true;
        options.Password.RequireDigit           = true;
        options.Password.RequireNonAlphanumeric = false;

        // Login por e-mail exige e-mail único
        options.User.RequireUniqueEmail = true;

        // Bloqueio por tentativas
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie de autenticação (RNF04 — expiração de sessão)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly  = true;                          // cookie inacessível via JS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // só trafega em HTTPS
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // <- precisa vir ANTES de UseAuthorization
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Cria os papéis e um Gestor inicial (idempotente)
await IdentitySeeder.SeedAsync(app.Services);

app.Run();

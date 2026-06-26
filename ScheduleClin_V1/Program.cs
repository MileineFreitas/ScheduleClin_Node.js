using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ScheduleClin.Context;
using ScheduleClin.Data;
using ScheduleClin.Models;

var builder = WebApplication.CreateBuilder(args);

// RNF06 (CSRF): valida o token antiforgery em TODA requisição que altera estado
// (POST/PUT/PATCH/DELETE), inclusive nas chamadas AJAX para os controllers de API.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// O token chega pelo header X-CSRF-TOKEN (enviado pelo fetch via _CsrfFetch)
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ScheduleClin API",
        Version = "v1",
        Description = "API para gestão de agendamentos, usuários e perfis"
    });
});

// RNF08 (auditoria): acesso ao usuário/IP da requisição + interceptor de SaveChanges
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

// dotnet user-secrets para mais segurança
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

// ───────── ASP.NET Core Identity ─────────
builder.Services
    .AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        // Política de senha (RNF03 — senha forte; o hash é feito pelo próprio Identity)
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        // Login por e-mail exige e-mail único
        options.User.RequireUniqueEmail = true;

        // Bloqueio por tentativas
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie de autenticação (RNF04 — expiração de sessão)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;                          // cookie inacessível via JS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // só trafega em HTTPS

    // APIs recebem 401/403 em vez de redirecionar para a tela de Login (HTML)
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ScheduleClin API v1");
    });
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // mapeia os controllers de API (ex: UserController)

// Cria os papéis e um Gestor inicial (idempotente)
await IdentitySeeder.SeedAsync(app.Services);

app.Run();

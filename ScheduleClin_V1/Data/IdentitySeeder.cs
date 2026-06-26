using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScheduleClin.Context;
using ScheduleClin.Models;

namespace ScheduleClin.Data;

// Cria papéis, perfis e usuário Gestor inicial na 1ª execução
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await SeedProfilesAsync(context);
        await SeedRolesAsync(roleManager);
        await SeedGestorAsync(userManager);
        await SyncExistingUsersAsync(context, userManager);
    }

    private static async Task SeedProfilesAsync(AppDbContext context)
    {
        var perfis = new[]
        {
            new Profile { ProfileId = ProfileIds.Gestor,    Name = Perfis.Gestor,    Description = "Administrador do sistema" },
            new Profile { ProfileId = ProfileIds.Psicologo, Name = Perfis.Psicologo, Description = "Funcionário — psicólogo(a)" },
            new Profile { ProfileId = ProfileIds.Paciente,  Name = Perfis.Paciente,  Description = "Paciente da clínica" },
        };

        foreach (var perfil in perfis)
        {
            if (!await context.Profiles.AnyAsync(p => p.ProfileId == perfil.ProfileId))
                context.Profiles.Add(perfil);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var papel in Perfis.Todos)
        {
            if (!await roleManager.RoleExistsAsync(papel))
                await roleManager.CreateAsync(new IdentityRole<Guid>(papel));
        }
    }

    private static async Task SeedGestorAsync(UserManager<User> userManager)
    {
        const string emailGestor = "gestor@scheduleclin.local";
        var gestor = await userManager.FindByEmailAsync(emailGestor);

        if (gestor is not null)
        {
            if (gestor.PerfilId is null)
            {
                gestor.PerfilId = ProfileIds.Gestor;
                await userManager.UpdateAsync(gestor);
            }
            if (!await userManager.IsInRoleAsync(gestor, Perfis.Gestor))
                await userManager.AddToRoleAsync(gestor, Perfis.Gestor);
            return;
        }

        gestor = new User
        {
            UserName           = emailGestor,
            Email              = emailGestor,
            EmailConfirmed     = true,
            PerfilId           = ProfileIds.Gestor,
            DataNascimento     = new DateTime(1990, 1, 1),
            MustChangePassword = true,
            Active             = true
        };

        var result = await userManager.CreateAsync(gestor, "Gestor@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(gestor, Perfis.Gestor);
    }

    // Alinha PerfilId ↔ Identity Role para usuários já existentes no banco
    private static async Task SyncExistingUsersAsync(AppDbContext context, UserManager<User> userManager)
    {
        var users   = await context.Users.Include(u => u.Perfil).ToListAsync();
        var profiles = await context.Profiles.ToListAsync();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Count > 0 && user.PerfilId is null)
            {
                var profile = profiles.FirstOrDefault(p => p.Name == roles[0]);
                if (profile is not null)
                {
                    user.PerfilId = profile.ProfileId;
                    await userManager.UpdateAsync(user);
                }
            }
            else if (user.PerfilId is not null && roles.Count == 0 && user.Perfil?.Name is not null)
            {
                await userManager.AddToRoleAsync(user, user.Perfil.Name);
            }
        }
    }
}

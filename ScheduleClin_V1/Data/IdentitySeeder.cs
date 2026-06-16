using Microsoft.AspNetCore.Identity;
using ScheduleClin.Models;

namespace ScheduleClin.Data;

// Cria os papéis padrão e um usuário Gestor inicial na 1ª execução
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // 1) Papéis (Gestor, Secretaria, Psicologo, Paciente)
        foreach (var papel in Perfis.Todos)
        {
            if (!await roleManager.RoleExistsAsync(papel))
                await roleManager.CreateAsync(new IdentityRole<Guid>(papel));
        }

        // 2) Gestor inicial — só cria se ainda não existir
        const string emailGestor = "gestor@scheduleclin.local";
        if (await userManager.FindByEmailAsync(emailGestor) is null)
        {
            var gestor = new User
            {
                UserName        = emailGestor,
                Email           = emailGestor,
                EmailConfirmed  = true,
                DataNascimento  = new DateTime(1990, 1, 1),
                MustChangePassword = true   // força troca no 1º acesso
            };

            var result = await userManager.CreateAsync(gestor, "Gestor@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(gestor, Perfis.Gestor);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScheduleClin.Context;
using ScheduleClin.Models;
using ScheduleClin.ViewModels;

namespace ScheduleClin.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;   // RNF08 — gravação de auditoria de login/logout

    public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, AppDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    // RF01 — tela de login
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var logged = await _userManager.GetUserAsync(User);
            if (logged is null)
                return RedirectToAction(nameof(Login));

            if (logged.MustChangePassword)
                return RedirectToAction(nameof(ChangePassword));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return await RedirecionarPorPerfil(logged);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]               // RNF06 — proteção CSRF
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.Active)
        {
            await RegistrarAuditAsync("LoginFalhou", user?.Id, model.Email, "Usuário inexistente ou inativo");
            ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            await RegistrarAuditAsync("LoginBloqueado", user.Id, user.Email, "Conta bloqueada por tentativas");
            ModelState.AddModelError(string.Empty, "Conta bloqueada temporariamente por excesso de tentativas.");
            return View(model);
        }
        if (!result.Succeeded)
        {
            await RegistrarAuditAsync("LoginFalhou", user.Id, user.Email, "Senha incorreta");
            ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
            return View(model);
        }

        await RegistrarAuditAsync("Login", user.Id, user.Email, null);

        // RF02 — primeiro acesso obriga troca de senha
        if (user.MustChangePassword)
            return RedirectToAction(nameof(ChangePassword));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return await RedirecionarPorPerfil(user);
    }

    // RF03 — logout
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var uid = _userManager.GetUserId(User);
        await RegistrarAuditAsync("Logout", Guid.TryParse(uid, out var g) ? g : null, User.Identity?.Name, null);

        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // RF02 — troca de senha
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        TempData["Msg"] = "Senha alterada com sucesso.";
        return await RedirecionarPorPerfil(user);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ───────── auxiliares ─────────
    private async Task<IActionResult> RedirecionarPorPerfil(User user)
    {
        if (await _userManager.IsInRoleAsync(user, Perfis.Gestor))
            return RedirectToAction("Index", "Admin");

        if (await _userManager.IsInRoleAsync(user, Perfis.Psicologo))
            return RedirectToAction("Agenda", "Psicologo");

        return RedirectToAction("Index", "Home");
    }

    private async Task RegistrarAuditAsync(string action, Guid? userId, string? userName, string? details)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Action    = action,
            UserId    = userId,
            UserName  = userName,
            Details   = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _context.SaveChangesAsync();
    }
}

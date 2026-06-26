using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleClin.Models;

namespace ScheduleClin.Controllers;

[Authorize] // exige usuário autenticado — demonstra o controle de acesso (RNF07)
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole(Perfis.Gestor))
            return RedirectToAction("Index", "Admin");

        if (User.IsInRole(Perfis.Psicologo))
            return RedirectToAction("Agenda", "Psicologo");

        return View();
    }
}

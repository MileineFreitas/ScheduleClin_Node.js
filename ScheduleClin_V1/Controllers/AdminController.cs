using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ScheduleClin.Controllers;

[Authorize(Roles = "Gestor")]
public class AdminController : Controller
{
    // GET /Admin  →  redireciona para Usuários (tela inicial do Figma)
    public IActionResult Index() => RedirectToAction(nameof(Usuarios));

    // GET /Admin/Usuarios
    public IActionResult Usuarios()    => View("Users");

    // GET /Admin/Agenda  (placeholder — implementar com módulo de agenda)
    public IActionResult Agenda()      => View("Calendar");

    // GET /Admin/Consultas  (placeholder)
    public IActionResult Consultas()   => View("Queries");

}

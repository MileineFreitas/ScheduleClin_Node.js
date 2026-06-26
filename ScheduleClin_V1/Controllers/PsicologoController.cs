using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleClin.Models;

namespace ScheduleClin.Controllers;

[Authorize(Roles = Perfis.Psicologo)]
public class PsicologoController : Controller
{
    // GET /Psicologo  →  agenda do psicólogo logado
    public IActionResult Index() => RedirectToAction(nameof(Agenda));

    // GET /Psicologo/Agenda
    public IActionResult Agenda() => View();

    // GET /Psicologo/Consultas
    public IActionResult Consultas() => View("Queries");

    // GET /Psicologo/Historico
    public IActionResult Historico() => View();
}

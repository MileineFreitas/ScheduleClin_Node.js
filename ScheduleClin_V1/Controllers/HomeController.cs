using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ScheduleClin.Controllers;

[Authorize] // exige usuário autenticado — demonstra o controle de acesso (RNF07)
public class HomeController : Controller
{
    public IActionResult Index() => View();
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleClin.Context;
using ScheduleClin.DTO;
using ScheduleClin.Models;
using Microsoft.AspNetCore.Authorization;

namespace ScheduleClin.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] //criar role para somente Secretária. adm ou Piscólogo podem acessar. 
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public UserController(AppDbContext context, UserManager<User> userManager   )
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                CPF = u.CPF,
                PerfilId = u.PerfilId,
                DataNascimento = u.DataNascimento,
                MustChangePassword = u.MustChangePassword,
                Active = u.Active
            })
            .ToListAsync();
        return Ok(users);
    }

    //chamada no front:  /api/users/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] StatusDto statusDto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if(user == null)
        {
            return BadRequest("Erro interno");
        }
        user.Active = statusDto.IsActive;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest("Erro ao atualizar status do usuário");
        }
        return Ok(new {id = user.Id, active = user.Active });
    }

}

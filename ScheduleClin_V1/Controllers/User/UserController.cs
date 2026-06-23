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

    [HttpGet("/get-users")]
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


    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto)
    {
        var perfilValido = await _context.Profiles.AnyAsync(p => p.ProfileId == dto.PerfilId
                && (p.Name == "Psicologo" || p.Name == "Secretária"));

        if (!perfilValido)
            return BadRequest(new { message = "Perfil inválido." });

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            CPF = dto.CPF,
            PerfilId = dto.PerfilId,
            DataNascimento = dto.DataNascimento,
            MustChangePassword = true,   // RF02 — obriga troca no primeiro acesso
            Active = true
        };

        //senha temporaria, alterar a´pos primeiro acesso
        var senhaProvisoria = GerarSenhaProvisoria();

        var result = await _userManager.CreateAsync(user, senhaProvisoria);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return CreatedAtAction(nameof(Guid), new { id = user.Id }, new
        {
            id = user.Id,
            userName = user.UserName,
            email = user.Email,
            senhaProvisoria
        });
    }

    // PUT: api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] UserEditDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return NotFound(new { message = "Usuário não encontrado." });

        if (dto.PerfilId.HasValue)
        {
            var perfilValido = await _context.Profiles.AnyAsync(p => p.ProfileId == dto.PerfilId
                    && (p.Name == "Psicologo" || p.Name == "Secretária"));

            if (!perfilValido)
                return BadRequest(new { message = "Perfil inválido." });
            user.PerfilId = dto.PerfilId;
        }

        if (!string.IsNullOrWhiteSpace(dto.UserName))
            user.UserName = dto.UserName;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            user.Email = dto.Email;

        if (dto.CPF is not null)
            user.CPF = dto.CPF;

        if (dto.DataNascimento.HasValue)
            user.DataNascimento = dto.DataNascimento.Value;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    private static string GerarSenhaProvisoria()
    {
        // Senha simples que atende as regras padrão do Identity
        // (1 maiúscula, 1 número, 1 símbolo, 8+ caracteres)
        return $"Prov@{Guid.NewGuid().ToString("N")[..6]}";
    }

}

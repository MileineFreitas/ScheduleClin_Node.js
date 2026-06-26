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
[Authorize(Roles = "Gestor")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public UserController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("/get-users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Perfil)
            .ToListAsync();

        var userRoles = await _context.UserRoles.ToListAsync();
        var roles     = await _context.Roles.ToListAsync();

        var dtos = users.Select(u =>
        {
            var roleId   = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleId;
            var roleName = roles.FirstOrDefault(r => r.Id == roleId)?.Name;

            return new UserDto
            {
                Id                 = u.Id,
                UserName           = u.UserName,
                Email              = u.Email,
                CPF                = u.CPF,
                Crp                = u.Crp,
                PerfilId           = u.PerfilId,
                Role               = roleName ?? u.Perfil?.Name,
                PerfilNome         = u.Perfil?.Name ?? roleName,
                DataNascimento     = u.DataNascimento,
                MustChangePassword = u.MustChangePassword,
                Active             = u.Active
            };
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("profiles")]
    public async Task<ActionResult<IEnumerable<ProfileDto>>> GetProfiles()
    {
        var profiles = await _context.Profiles
            .Where(p => Perfis.Cadastro.Contains(p.Name))
            .Select(p => new ProfileDto
            {
                ProfileId   = p.ProfileId,
                Name        = p.Name,
                Description = p.Description
            })
            .ToListAsync();

        return Ok(profiles);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] StatusDto statusDto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return BadRequest(new { message = "Usuário não encontrado." });

        user.Active = statusDto.IsActive;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { message = "Erro ao atualizar status do usuário." });

        return Ok(new { id = user.Id, active = user.Active });
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] UserCreateDto dto)
    {
        var perfil = await _context.Profiles
            .FirstOrDefaultAsync(p => p.ProfileId == dto.PerfilId);

        if (perfil is null || !Perfis.Cadastro.Contains(perfil.Name))
            return BadRequest(new { message = "Perfil inválido. Use Psicologo ou Paciente." });

        if (perfil.Name == Perfis.Psicologo && string.IsNullOrWhiteSpace(dto.Crp))
            return BadRequest(new { message = "CRP é obrigatório para psicólogo(a)." });

        if (!string.IsNullOrWhiteSpace(dto.CPF) && await _context.Users.AnyAsync(u => u.CPF == dto.CPF))
            return BadRequest(new { message = "Já existe um usuário cadastrado com esse CPF." });

        if (!string.IsNullOrWhiteSpace(dto.Crp) && await _context.Users.AnyAsync(u => u.Crp == dto.Crp.Trim()))
            return BadRequest(new { message = "Já existe um usuário cadastrado com esse CRP." });

        var user = new User
        {
            UserName           = dto.UserName,
            Email              = dto.Email,
            CPF                = dto.CPF,
            Crp                = perfil.Name == Perfis.Psicologo ? dto.Crp?.Trim() : null,
            PerfilId           = dto.PerfilId,
            DataNascimento     = dto.DataNascimento,
            MustChangePassword = true,
            Active             = true
        };

        var senhaProvisoria = GerarSenhaProvisoria();
        var result = await _userManager.CreateAsync(user, senhaProvisoria);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, perfil.Name);

        return Created($"/api/User/{user.Id}", new
        {
            id = user.Id,
            userName = user.UserName,
            email = user.Email,
            role = perfil.Name,
            senhaProvisoria
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] UserEditDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return NotFound(new { message = "Usuário não encontrado." });

        if (dto.PerfilId.HasValue)
        {
            var perfil = await _context.Profiles
                .FirstOrDefaultAsync(p => p.ProfileId == dto.PerfilId);

            if (perfil is null || !Perfis.Cadastro.Contains(perfil.Name))
                return BadRequest(new { message = "Perfil inválido." });

            user.PerfilId = dto.PerfilId;

            var rolesAtuais = await _userManager.GetRolesAsync(user);
            if (rolesAtuais.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, rolesAtuais);

            await _userManager.AddToRoleAsync(user, perfil.Name);
        }

        if (!string.IsNullOrWhiteSpace(dto.UserName))
            user.UserName = dto.UserName;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            user.Email = dto.Email;

        if (dto.CPF is not null)
        {
            if (!string.IsNullOrWhiteSpace(dto.CPF) && await _context.Users.AnyAsync(u => u.CPF == dto.CPF && u.Id != id))
                return BadRequest(new { message = "Já existe um usuário cadastrado com esse CPF." });

            user.CPF = dto.CPF;
        }

        if (dto.DataNascimento.HasValue)
            user.DataNascimento = dto.DataNascimento.Value;

        if (dto.Crp is not null)
        {
            var crp = string.IsNullOrWhiteSpace(dto.Crp) ? null : dto.Crp.Trim();

            if (crp is not null && await _context.Users.AnyAsync(u => u.Crp == crp && u.Id != id))
                return BadRequest(new { message = "Já existe um usuário cadastrado com esse CRP." });

            user.Crp = crp;
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpPatch("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new { message = "Usuário não encontrado." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var senhaProvisoria = GerarSenhaProvisoria();
        var result = await _userManager.ResetPasswordAsync(user, token, senhaProvisoria);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        return Ok(new { id = user.Id, senhaProvisoria });
    }

    private static string GerarSenhaProvisoria()
    {
        return $"Prov@{Guid.NewGuid().ToString("N")[..6]}";
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleClin.Context;
using ScheduleClin.DTO;
using ScheduleClin.Models;

namespace ScheduleClin.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Gestor")]
public class CalendarController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public CalendarController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarDto>>> GetCalendars(DateTime? inicio, DateTime? fim)
    {
        var query = _context.Calendars.AsQueryable();

        if (inicio.HasValue)
            query = query.Where(c => c.ScheduleDate >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(c => c.ScheduleDate <= fim.Value);

        var consultas = await query.OrderBy(c => c.ScheduleDate).ToListAsync();

        var userIds = consultas
            .SelectMany(c => new[] { c.PacienteId, c.PsicologoId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var usuarios = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        var dtos = consultas.Select(c => new CalendarDto
        {
            CalendarId      = c.CalendarID,
            Title           = c.Title,
            ScheduleDate    = c.ScheduleDate,
            DurationMinutes = c.DurationMinutes,
            Status          = c.Status,
            PacienteId      = c.PacienteId,
            PacienteNome    = c.PacienteId.HasValue ? usuarios.GetValueOrDefault(c.PacienteId.Value) : null,
            PsicologoId     = c.PsicologoId,
            PsicologoNome   = c.PsicologoId.HasValue ? usuarios.GetValueOrDefault(c.PsicologoId.Value) : null,
        });

        return Ok(dtos);
    }

    [HttpGet("pacientes")]
    public async Task<ActionResult> GetPacientes()
    {
        var pacientes = await _context.Users
            .Include(u => u.Perfil)
            .Where(u => u.Active && u.Perfil != null && u.Perfil.Name == Perfis.Paciente)
            .Select(u => new { id = u.Id, nome = u.UserName })
            .ToListAsync();

        return Ok(pacientes);
    }

    [HttpGet("psicologos")]
    public async Task<ActionResult> GetPsicologos()
    {
        var psicologos = await _context.Users
            .Include(u => u.Perfil)
            .Where(u => u.Active && u.Perfil != null && u.Perfil.Name == Perfis.Psicologo)
            .Select(u => new { id = u.Id, nome = u.UserName })
            .ToListAsync();

        return Ok(psicologos);
    }

    private static readonly TimeSpan HoraAbertura = TimeSpan.FromHours(7);
    private static readonly TimeSpan HoraFechamento = TimeSpan.FromHours(19);

    private static bool ForaDoHorarioComercial(DateTime scheduleDate)
    {
        var hora = scheduleDate.TimeOfDay;
        return hora < HoraAbertura || hora >= HoraFechamento;
    }

    private async Task<bool> PsicologoTemConflito(Guid psicologoId, DateTime scheduleDate, Guid? ignorarId = null)
    {
        return await _context.Calendars.AnyAsync(c =>
            c.PsicologoId == psicologoId &&
            c.ScheduleDate == scheduleDate &&
            c.Status != AppointmentStatus.Cancelada &&
            c.CalendarID != ignorarId);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CalendarCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Título é obrigatório." });

        if (ForaDoHorarioComercial(dto.ScheduleDate))
            return BadRequest(new { message = "Só é possível agendar consultas entre 07:00 e 19:59." });

        if (dto.PsicologoId.HasValue && await PsicologoTemConflito(dto.PsicologoId.Value, dto.ScheduleDate))
            return BadRequest(new { message = "Esse psicólogo(a) já tem uma consulta nesse mesmo dia e horário." });

        var criadoPorId = Guid.Parse(_userManager.GetUserId(User)!);

        var calendar = new Calendar
        {
            CalendarID      = Guid.NewGuid(),
            Title           = dto.Title,
            ScheduleDate    = dto.ScheduleDate,
            DurationMinutes = dto.DurationMinutes,
            PacienteId      = dto.PacienteId,
            PsicologoId     = dto.PsicologoId,
            Status          = AppointmentStatus.Confirmada,
            CriadoPorId     = criadoPorId,
        };

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();

        return Created($"/api/Calendar/{calendar.CalendarID}", new { id = calendar.CalendarID });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] CalendarEditDto dto)
    {
        var calendar = await _context.Calendars.FindAsync(id);
        if (calendar is null)
            return NotFound(new { message = "Consulta não encontrada." });

        if (calendar.Status == AppointmentStatus.Cancelada)
            return BadRequest(new { message = "Não é possível editar uma consulta cancelada." });

        var novaData = dto.ScheduleDate ?? calendar.ScheduleDate;
        var novoPsicologoId = dto.PsicologoId ?? calendar.PsicologoId;

        if (ForaDoHorarioComercial(novaData))
            return BadRequest(new { message = "Só é possível agendar consultas entre 07:00 e 19:59." });

        if (novoPsicologoId.HasValue && await PsicologoTemConflito(novoPsicologoId.Value, novaData, calendar.CalendarID))
            return BadRequest(new { message = "Esse psicólogo(a) já tem uma consulta nesse mesmo dia e horário." });

        if (!string.IsNullOrWhiteSpace(dto.Title))
            calendar.Title = dto.Title;

        calendar.ScheduleDate = novaData;
        calendar.PsicologoId = novoPsicologoId;

        if (dto.DurationMinutes.HasValue)
            calendar.DurationMinutes = dto.DurationMinutes.Value;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!AppointmentStatus.Todos.Contains(dto.Status))
                return BadRequest(new { message = "Status inválido." });
            calendar.Status = dto.Status;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var calendar = await _context.Calendars.FindAsync(id);
        if (calendar is null)
            return NotFound(new { message = "Consulta não encontrada." });

        calendar.Status = AppointmentStatus.Cancelada;
        await _context.SaveChangesAsync();

        return Ok(new { id = calendar.CalendarID, status = calendar.Status });
    }
}

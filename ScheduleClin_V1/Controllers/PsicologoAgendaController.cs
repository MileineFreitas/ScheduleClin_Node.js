using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleClin.Context;
using ScheduleClin.DTO;
using ScheduleClin.Models;

namespace ScheduleClin.Controllers;

// API da agenda do psicólogo logado (consultas próprias: listar, criar, editar e cancelar).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Perfis.Psicologo)]
public class PsicologoAgendaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public PsicologoAgendaController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarDto>>> GetMinhaAgenda(DateTime? inicio, DateTime? fim, bool desdeHoje = false)
    {
        var psicologoId = Guid.Parse(_userManager.GetUserId(User)!);

        var query = _context.Calendars
            .Where(c => c.PsicologoId == psicologoId);

        if (desdeHoje)
            query = query.Where(c => c.ScheduleDate.Date >= DateTime.Today);
        else if (inicio.HasValue)
            query = query.Where(c => c.ScheduleDate >= inicio.Value);

        if (fim.HasValue)
            query = query.Where(c => c.ScheduleDate <= fim.Value);

        var consultas = await query.OrderBy(c => c.ScheduleDate).ToListAsync();

        var pacienteIds = consultas
            .Where(c => c.PacienteId.HasValue)
            .Select(c => c.PacienteId!.Value)
            .Distinct()
            .ToList();

        var pacientes = await _context.Users
            .Where(u => pacienteIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        var nomePsicologo = (await _userManager.GetUserAsync(User))?.UserName;

        var dtos = consultas.Select(c => new CalendarDto
        {
            CalendarId      = c.CalendarID,
            Title           = c.Title,
            ScheduleDate    = c.ScheduleDate,
            DurationMinutes = c.DurationMinutes,
            Status          = c.Status,
            PacienteId      = c.PacienteId,
            PacienteNome    = c.PacienteId.HasValue ? pacientes.GetValueOrDefault(c.PacienteId.Value) : null,
            PsicologoId     = c.PsicologoId,
            PsicologoNome   = nomePsicologo,
        });

        return Ok(dtos);
    }

    [HttpGet("historico")]
    public async Task<ActionResult<IEnumerable<CalendarDto>>> GetHistorico()
    {
        var psicologoId = Guid.Parse(_userManager.GetUserId(User)!);
        var agora = DateTime.Now;

        var consultas = await _context.Calendars
            .Where(c => c.PsicologoId == psicologoId)
            .Where(c => c.PacienteId.HasValue)
            .Where(c => c.ScheduleDate <= agora || c.Status == AppointmentStatus.Finalizado)
            .OrderByDescending(c => c.ScheduleDate)
            .ToListAsync();

        var pacienteIds = consultas
            .Select(c => c.PacienteId!.Value)
            .Distinct()
            .ToList();

        var pacientes = await _context.Users
            .Where(u => pacienteIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        var nomePsicologo = (await _userManager.GetUserAsync(User))?.UserName;

        var dtos = consultas.Select(c => new CalendarDto
        {
            CalendarId      = c.CalendarID,
            Title           = c.Title,
            ScheduleDate    = c.ScheduleDate,
            DurationMinutes = c.DurationMinutes,
            Status          = c.Status,
            PacienteId      = c.PacienteId,
            PacienteNome    = pacientes.GetValueOrDefault(c.PacienteId!.Value),
            PsicologoId     = c.PsicologoId,
            PsicologoNome   = nomePsicologo,
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
            c.Status != AppointmentStatus.Finalizado &&
            c.CalendarID != ignorarId);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CalendarCreateDto dto)
    {
        var psicologoId = Guid.Parse(_userManager.GetUserId(User)!);

        if (!dto.PacienteId.HasValue)
            return BadRequest(new { message = "Paciente é obrigatório." });

        if (dto.ScheduleDate.Date < DateTime.Today)
            return BadRequest(new { message = "Não é possível agendar consultas em datas anteriores." });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Título é obrigatório." });

        if (ForaDoHorarioComercial(dto.ScheduleDate))
            return BadRequest(new { message = "Só é possível agendar consultas entre 07:00 e 19:59." });

        if (await PsicologoTemConflito(psicologoId, dto.ScheduleDate))
            return BadRequest(new { message = "Você já tem uma consulta nesse mesmo dia e horário." });

        var calendar = new Calendar
        {
            CalendarID      = Guid.NewGuid(),
            Title           = dto.Title,
            ScheduleDate    = dto.ScheduleDate,
            DurationMinutes = dto.DurationMinutes,
            PacienteId      = dto.PacienteId,
            PsicologoId     = psicologoId,
            Status          = AppointmentStatus.Confirmada,
            CriadoPorId     = psicologoId,
        };

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();

        return Created($"/api/PsicologoAgenda/{calendar.CalendarID}", new { id = calendar.CalendarID });
    }

    private async Task<Calendar?> GetConsultaDoPsicologo(Guid id, Guid psicologoId) =>
        await _context.Calendars.FirstOrDefaultAsync(c =>
            c.CalendarID == id &&
            c.PsicologoId == psicologoId &&
            c.PacienteId.HasValue);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] CalendarEditDto dto)
    {
        var psicologoId = Guid.Parse(_userManager.GetUserId(User)!);
        var calendar = await GetConsultaDoPsicologo(id, psicologoId);

        if (calendar is null)
            return NotFound(new { message = "Consulta não encontrada." });

        if (calendar.Status == AppointmentStatus.Cancelada)
            return BadRequest(new { message = "Não é possível editar uma consulta cancelada." });

        if (calendar.Status == AppointmentStatus.Finalizado)
            return BadRequest(new { message = "Não é possível editar uma consulta finalizada." });

        var novaData = dto.ScheduleDate ?? calendar.ScheduleDate;

        if (novaData.Date < DateTime.Today)
            return BadRequest(new { message = "Não é possível agendar consultas em datas anteriores." });

        if (ForaDoHorarioComercial(novaData))
            return BadRequest(new { message = "Só é possível agendar consultas entre 07:00 e 19:59." });

        if (await PsicologoTemConflito(psicologoId, novaData, calendar.CalendarID))
            return BadRequest(new { message = "Você já tem uma consulta nesse mesmo dia e horário." });

        if (!string.IsNullOrWhiteSpace(dto.Title))
            calendar.Title = dto.Title;

        calendar.ScheduleDate = novaData;

        if (dto.DurationMinutes.HasValue)
            calendar.DurationMinutes = dto.DurationMinutes.Value;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (dto.Status == AppointmentStatus.Finalizado)
                return BadRequest(new { message = "Use a ação de finalizar consulta." });

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
        var psicologoId = Guid.Parse(_userManager.GetUserId(User)!);
        var calendar = await GetConsultaDoPsicologo(id, psicologoId);

        if (calendar is null)
            return NotFound(new { message = "Consulta não encontrada." });

        if (calendar.Status == AppointmentStatus.Cancelada)
            return BadRequest(new { message = "Esta consulta já está cancelada." });

        calendar.Status = AppointmentStatus.Cancelada;
        await _context.SaveChangesAsync();

        return Ok(new { id = calendar.CalendarID, status = calendar.Status });
    }

    [HttpPatch("{id:guid}/finalizar")]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        var psicologoId = Guid.Parse(_userManager.GetUserId(User)!);
        var calendar = await GetConsultaDoPsicologo(id, psicologoId);

        if (calendar is null)
            return NotFound(new { message = "Consulta não encontrada." });

        if (calendar.Status == AppointmentStatus.Cancelada)
            return BadRequest(new { message = "Não é possível finalizar uma consulta cancelada." });

        if (calendar.Status == AppointmentStatus.Finalizado)
            return BadRequest(new { message = "Esta consulta já está finalizada." });

        calendar.Status = AppointmentStatus.Finalizado;
        await _context.SaveChangesAsync();

        return Ok(new { id = calendar.CalendarID, status = calendar.Status });
    }
}

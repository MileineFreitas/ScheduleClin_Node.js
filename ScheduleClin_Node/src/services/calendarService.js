const { randomUUID } = require('crypto');
const { getPrisma } = require('../utils/prisma');
const { Perfis } = require('../utils/perfis');
const { AppointmentStatus } = require('../utils/appointmentStatus');

const HORA_ABERTURA = 7;
const HORA_FECHAMENTO = 19;

function foraDoHorarioComercial(scheduleDate) {
  const d = new Date(scheduleDate);
  const hora = d.getHours();
  const min = d.getMinutes();
  const time = hora + min / 60;
  return time < HORA_ABERTURA || time >= HORA_FECHAMENTO;
}

async function psicologoTemConflito(psicologoId, scheduleDate, ignorarId = null) {
  const prisma = getPrisma();
  const where = {
    psicologoId,
    scheduleDate: new Date(scheduleDate),
    status: { notIn: [AppointmentStatus.Cancelada, AppointmentStatus.Finalizado] },
  };
  if (ignorarId) where.calendarID = { not: ignorarId };
  return (await prisma.calendar.count({ where })) > 0;
}

async function mapCalendars(consultas) {
  const prisma = getPrisma();
  const userIds = [...new Set(
    consultas.flatMap((c) => [c.pacienteId, c.psicologoId].filter(Boolean)),
  )];

  const usuarios = userIds.length
    ? await prisma.user.findMany({ where: { id: { in: userIds } } })
    : [];

  const byId = Object.fromEntries(usuarios.map((u) => [u.id, u.userName]));

  return consultas.map((c) => ({
    calendarId: c.calendarID,
    title: c.title,
    scheduleDate: c.scheduleDate,
    durationMinutes: c.durationMinutes,
    status: c.status,
    pacienteId: c.pacienteId,
    pacienteNome: c.pacienteId ? byId[c.pacienteId] : null,
    psicologoId: c.psicologoId,
    psicologoNome: c.psicologoId ? byId[c.psicologoId] : null,
  }));
}

async function getCalendars(inicio, fim) {
  const prisma = getPrisma();
  const where = {};
  if (inicio) where.scheduleDate = { ...where.scheduleDate, gte: new Date(inicio) };
  if (fim) where.scheduleDate = { ...where.scheduleDate, lte: new Date(fim) };

  const consultas = await prisma.calendar.findMany({
    where,
    orderBy: { scheduleDate: 'asc' },
  });
  return mapCalendars(consultas);
}

async function getPacientesAtivos() {
  const prisma = getPrisma();
  return prisma.user.findMany({
    where: { active: true, perfil: { name: Perfis.Paciente } },
    select: { id: true, userName: true },
  }).then((rows) => rows.map((u) => ({ id: u.id, nome: u.userName })));
}

async function getPsicologosAtivos() {
  const prisma = getPrisma();
  return prisma.user.findMany({
    where: { active: true, perfil: { name: Perfis.Psicologo } },
    select: { id: true, userName: true },
  }).then((rows) => rows.map((u) => ({ id: u.id, nome: u.userName })));
}

async function createCalendar(dto, criadoPorId) {
  if (!dto.title?.trim()) return { ok: false, status: 400, message: 'Título é obrigatório.' };
  if (foraDoHorarioComercial(dto.scheduleDate)) {
    return { ok: false, status: 400, message: 'Só é possível agendar consultas entre 07:00 e 19:59.' };
  }
  if (dto.psicologoId && await psicologoTemConflito(dto.psicologoId, dto.scheduleDate)) {
    return { ok: false, status: 400, message: 'Esse psicólogo(a) já tem uma consulta nesse mesmo dia e horário.' };
  }

  const prisma = getPrisma();
  const calendar = await prisma.calendar.create({
    data: {
      calendarID: randomUUID(),
      title: dto.title,
      scheduleDate: new Date(dto.scheduleDate),
      durationMinutes: dto.durationMinutes || 60,
      pacienteId: dto.pacienteId || null,
      psicologoId: dto.psicologoId || null,
      status: AppointmentStatus.Confirmada,
      criadoPorId,
    },
  });

  return { ok: true, status: 201, body: { id: calendar.calendarID } };
}

async function editCalendar(id, dto, options = {}) {
  const prisma = getPrisma();
  const calendar = await prisma.calendar.findUnique({ where: { calendarID: id } });
  if (!calendar) return { ok: false, status: 404, message: 'Consulta não encontrada.' };

  if (calendar.status === AppointmentStatus.Cancelada) {
    return { ok: false, status: 400, message: 'Não é possível editar uma consulta cancelada.' };
  }
  if (calendar.status === AppointmentStatus.Finalizado) {
    return { ok: false, status: 400, message: 'Não é possível editar uma consulta finalizada.' };
  }

  const novaData = dto.scheduleDate ? new Date(dto.scheduleDate) : calendar.scheduleDate;
  const novoPsicologoId = dto.psicologoId !== undefined ? dto.psicologoId : calendar.psicologoId;

  if (options.blockPastDates && novaData < startOfToday()) {
    return { ok: false, status: 400, message: 'Não é possível agendar consultas em datas anteriores.' };
  }
  if (foraDoHorarioComercial(novaData)) {
    return { ok: false, status: 400, message: 'Só é possível agendar consultas entre 07:00 e 19:59.' };
  }
  if (novoPsicologoId && await psicologoTemConflito(novoPsicologoId, novaData, id)) {
    const msg = options.psicologoSelf
      ? 'Você já tem uma consulta nesse mesmo dia e horário.'
      : 'Esse psicólogo(a) já tem uma consulta nesse mesmo dia e horário.';
    return { ok: false, status: 400, message: msg };
  }

  const data = { scheduleDate: novaData, psicologoId: novoPsicologoId };
  if (dto.title?.trim()) data.title = dto.title;
  if (dto.durationMinutes) data.durationMinutes = dto.durationMinutes;

  if (dto.status?.trim()) {
    if (dto.status === AppointmentStatus.Finalizado && !options.allowFinalizar) {
      return { ok: false, status: 400, message: options.finalizarMessage || 'Apenas o psicólogo(a) responsável pode finalizar a consulta.' };
    }
    if (!AppointmentStatus.Todos.includes(dto.status)) {
      return { ok: false, status: 400, message: 'Status inválido.' };
    }
    data.status = dto.status;
  }

  await prisma.calendar.update({ where: { calendarID: id }, data });
  return { ok: true, status: 204 };
}

async function cancelCalendar(id, filter = {}) {
  const prisma = getPrisma();
  const calendar = await prisma.calendar.findFirst({ where: { calendarID: id, ...filter } });
  if (!calendar) return { ok: false, status: 404, message: 'Consulta não encontrada.' };

  if (calendar.status === AppointmentStatus.Cancelada) {
    return { ok: false, status: 400, message: 'Esta consulta já está cancelada.' };
  }
  if (calendar.status === AppointmentStatus.Finalizado) {
    return { ok: false, status: 400, message: 'Não é possível cancelar uma consulta já finalizada.' };
  }

  await prisma.calendar.update({
    where: { calendarID: id },
    data: { status: AppointmentStatus.Cancelada },
  });
  return { ok: true, body: { id, status: AppointmentStatus.Cancelada } };
}

async function finalizarCalendar(id, psicologoId) {
  const prisma = getPrisma();
  const calendar = await prisma.calendar.findFirst({
    where: { calendarID: id, psicologoId, pacienteId: { not: null } },
  });
  if (!calendar) return { ok: false, status: 404, message: 'Consulta não encontrada.' };
  if (calendar.status === AppointmentStatus.Cancelada) {
    return { ok: false, status: 400, message: 'Não é possível finalizar uma consulta cancelada.' };
  }
  if (calendar.status === AppointmentStatus.Finalizado) {
    return { ok: false, status: 400, message: 'Esta consulta já está finalizada.' };
  }

  await prisma.calendar.update({
    where: { calendarID: id },
    data: { status: AppointmentStatus.Finalizado },
  });
  return { ok: true, body: { id, status: AppointmentStatus.Finalizado } };
}

function startOfToday() {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  return d;
}

async function getMinhaAgendaPsicologo(psicologoId, { inicio, fim, desdeHoje }) {
  const prisma = getPrisma();
  const where = { psicologoId };

  if (desdeHoje) {
    where.scheduleDate = { gte: startOfToday() };
  } else if (inicio) {
    where.scheduleDate = { gte: new Date(inicio) };
  }
  if (fim) where.scheduleDate = { ...where.scheduleDate, lte: new Date(fim) };

  const consultas = await prisma.calendar.findMany({ where, orderBy: { scheduleDate: 'asc' } });
  const psicologo = await prisma.user.findUnique({ where: { id: psicologoId } });
  const dtos = await mapCalendars(consultas);
  return dtos.map((d) => ({ ...d, psicologoNome: psicologo?.userName }));
}

async function getHistoricoPsicologo(psicologoId) {
  const prisma = getPrisma();
  const agora = new Date();
  const consultas = await prisma.calendar.findMany({
    where: {
      psicologoId,
      pacienteId: { not: null },
      OR: [
        { scheduleDate: { lte: agora } },
        { status: AppointmentStatus.Finalizado },
      ],
    },
    orderBy: { scheduleDate: 'desc' },
  });
  const psicologo = await prisma.user.findUnique({ where: { id: psicologoId } });
  const dtos = await mapCalendars(consultas);
  return dtos.map((d) => ({ ...d, psicologoNome: psicologo?.userName }));
}

async function createPsicologoAgenda(dto, psicologoId) {
  if (!dto.pacienteId) return { ok: false, status: 400, message: 'Paciente é obrigatório.' };
  if (new Date(dto.scheduleDate) < startOfToday()) {
    return { ok: false, status: 400, message: 'Não é possível agendar consultas em datas anteriores.' };
  }
  return createCalendar({ ...dto, psicologoId }, psicologoId);
}

async function getMinhasConsultasPaciente(pacienteId) {
  const prisma = getPrisma();
  const consultas = await prisma.calendar.findMany({
    where: { pacienteId },
    orderBy: { scheduleDate: 'asc' },
  });
  return mapCalendars(consultas);
}

async function getHistoricoPaciente(pacienteId) {
  const prisma = getPrisma();
  const agora = new Date();
  const consultas = await prisma.calendar.findMany({
    where: {
      pacienteId,
      OR: [
        { scheduleDate: { lt: agora } },
        { status: { in: [AppointmentStatus.Cancelada, AppointmentStatus.Finalizado] } },
      ],
    },
    orderBy: { scheduleDate: 'desc' },
  });
  return mapCalendars(consultas);
}

async function createPacienteAgenda(dto, pacienteId, pacienteNome) {
  if (!dto.psicologoId) return { ok: false, status: 400, message: 'Psicólogo é obrigatório.' };
  if (new Date(dto.scheduleDate) < startOfToday()) {
    return { ok: false, status: 400, message: 'Não é possível agendar em datas anteriores.' };
  }
  if (foraDoHorarioComercial(dto.scheduleDate)) {
    return { ok: false, status: 400, message: 'Só é possível agendar entre 07:00 e 19:00.' };
  }

  const prisma = getPrisma();
  const conflito = await prisma.calendar.count({
    where: {
      psicologoId: dto.psicologoId,
      scheduleDate: new Date(dto.scheduleDate),
      status: { notIn: [AppointmentStatus.Cancelada, AppointmentStatus.Finalizado] },
    },
  });
  if (conflito) {
    return { ok: false, status: 400, message: 'Esse psicólogo já tem uma consulta nesse dia e horário.' };
  }

  const consulta = await prisma.calendar.create({
    data: {
      calendarID: randomUUID(),
      title: dto.title || `Consulta – ${pacienteNome}`,
      scheduleDate: new Date(dto.scheduleDate),
      durationMinutes: dto.durationMinutes > 0 ? dto.durationMinutes : 60,
      pacienteId,
      psicologoId: dto.psicologoId,
      status: AppointmentStatus.Pendente,
      criadoPorId: pacienteId,
    },
  });

  return { ok: true, status: 201, body: { id: consulta.calendarID } };
}

module.exports = {
  getCalendars,
  getPacientesAtivos,
  getPsicologosAtivos,
  createCalendar,
  editCalendar,
  cancelCalendar,
  finalizarCalendar,
  getMinhaAgendaPsicologo,
  getHistoricoPsicologo,
  createPsicologoAgenda,
  getMinhasConsultasPaciente,
  getHistoricoPaciente,
  createPacienteAgenda,
  startOfToday,
  foraDoHorarioComercial,
  psicologoTemConflito,
};

const express = require('express');
const { requireRole } = require('../../middleware/auth');
const calendarService = require('../../services/calendarService');
const { AppointmentStatus } = require('../../utils/appointmentStatus');
const { getPrisma } = require('../../utils/prisma');

const router = express.Router();

router.get('/', requireRole('Psicologo'), async (req, res) => {
  try {
    const data = await calendarService.getMinhaAgendaPsicologo(req.session.userId, {
      inicio: req.query.inicio,
      fim: req.query.fim,
      desdeHoje: req.query.desdeHoje === 'true',
    });
    res.json(data);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.get('/historico', requireRole('Psicologo'), async (req, res) => {
  try {
    res.json(await calendarService.getHistoricoPsicologo(req.session.userId));
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.get('/pacientes', requireRole('Psicologo'), async (req, res) => {
  try {
    res.json(await calendarService.getPacientesAtivos());
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.post('/', requireRole('Psicologo'), async (req, res) => {
  try {
    const result = await calendarService.createPsicologoAgenda(req.body, req.session.userId);
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.status(result.status).location(`/api/PsicologoAgenda/${result.body.id}`).json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.put('/:id', requireRole('Psicologo'), async (req, res) => {
  try {
    const prisma = getPrisma();
    const exists = await prisma.calendar.findFirst({
      where: {
        calendarID: req.params.id,
        psicologoId: req.session.userId,
        pacienteId: { not: null },
      },
    });
    if (!exists) return res.status(404).json({ message: 'Consulta não encontrada.' });

    const result = await calendarService.editCalendar(req.params.id, req.body, {
      blockPastDates: true,
      psicologoSelf: true,
      allowFinalizar: false,
      finalizarMessage: 'Use a ação de finalizar consulta.',
    });
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.sendStatus(result.status);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.patch('/:id/cancel', requireRole('Psicologo'), async (req, res) => {
  try {
    const result = await calendarService.cancelCalendar(req.params.id, {
      psicologoId: req.session.userId,
      pacienteId: { not: null },
    });
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.patch('/:id/finalizar', requireRole('Psicologo'), async (req, res) => {
  try {
    const result = await calendarService.finalizarCalendar(req.params.id, req.session.userId);
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

module.exports = router;

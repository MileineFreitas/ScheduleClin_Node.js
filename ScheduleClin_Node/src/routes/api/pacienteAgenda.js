const express = require('express');
const { requireRole } = require('../../middleware/auth');
const calendarService = require('../../services/calendarService');
const { getPrisma } = require('../../utils/prisma');

const router = express.Router();

router.get('/', requireRole('Paciente'), async (req, res) => {
  try {
    res.json(await calendarService.getMinhasConsultasPaciente(req.session.userId));
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.get('/historico', requireRole('Paciente'), async (req, res) => {
  try {
    res.json(await calendarService.getHistoricoPaciente(req.session.userId));
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.get('/psicologos', requireRole('Paciente'), async (req, res) => {
  try {
    res.json(await calendarService.getPsicologosAtivos());
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.post('/', requireRole('Paciente'), async (req, res) => {
  try {
    const result = await calendarService.createPacienteAgenda(
      req.body,
      req.session.userId,
      req.session.userName,
    );
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.status(result.status).location(`/api/PacienteAgenda/${result.body.id}`).json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.patch('/:id/cancel', requireRole('Paciente'), async (req, res) => {
  try {
    const result = await calendarService.cancelCalendar(req.params.id, {
      pacienteId: req.session.userId,
    });
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

module.exports = router;

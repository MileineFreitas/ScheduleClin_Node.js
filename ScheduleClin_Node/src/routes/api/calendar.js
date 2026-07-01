const express = require('express');
const { requireRole } = require('../../middleware/auth');
const calendarService = require('../../services/calendarService');

const router = express.Router();

router.get('/', requireRole('Gestor'), async (req, res) => {
  try {
    const data = await calendarService.getCalendars(req.query.inicio, req.query.fim);
    res.json(data);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.get('/pacientes', requireRole('Gestor'), async (req, res) => {
  try {
    res.json(await calendarService.getPacientesAtivos());
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.get('/psicologos', requireRole('Gestor'), async (req, res) => {
  try {
    res.json(await calendarService.getPsicologosAtivos());
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.post('/', requireRole('Gestor'), async (req, res) => {
  try {
    const result = await calendarService.createCalendar(req.body, req.session.userId);
    if (!result.ok) {return res.status(result.status).json({ message: result.message });}
    res.status(result.status).location(`/api/Calendar/${result.body.id}`).json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.put('/:id', requireRole('Gestor'), async (req, res) => {
  try {
    const result = await calendarService.editCalendar(req.params.id, req.body);
    if (!result.ok) {return res.status(result.status).json({ message: result.message });}
    res.sendStatus(result.status);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.patch('/:id/cancel', requireRole('Gestor'), async (req, res) => {
  try {
    const result = await calendarService.cancelCalendar(req.params.id);
    if (!result.ok) {return res.status(result.status).json({ message: result.message });}
    res.json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

module.exports = router;

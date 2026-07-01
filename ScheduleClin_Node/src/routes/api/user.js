const express = require('express');
const { requireRole } = require('../../middleware/auth');
const userService = require('../../services/userService');

const router = express.Router();

router.get('/profiles', requireRole('Gestor'), async (req, res) => {
  try {
    const profiles = await userService.getProfiles();
    res.json(profiles);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.post('/', requireRole('Gestor'), async (req, res) => {
  try {
    const result = await userService.createUser(req.body);
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.status(result.status).location(`/api/User/${result.body.id}`).json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.put('/:id', requireRole('Gestor'), async (req, res) => {
  try {
    const result = await userService.updateUser(req.params.id, req.body);
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.sendStatus(result.status);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.patch('/:id/status', requireRole('Gestor'), async (req, res) => {
  try {
    const result = await userService.alterarStatus(req.params.id, req.body.isActive);
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

router.patch('/:id/reset-password', requireRole('Gestor'), async (req, res) => {
  try {
    const result = await userService.resetPassword(req.params.id);
    if (!result.ok) return res.status(result.status).json({ message: result.message });
    res.json(result.body);
  } catch (err) {
    console.error(err);
    res.status(500).json({ message: 'Erro interno.' });
  }
});

module.exports = router;

const express = require('express');
const { requireRole } = require('../../middleware/auth');

const router = express.Router();

router.get('/', requireRole('Paciente'), (req, res) => res.redirect('/paciente/consultas'));

router.get('/consultas', requireRole('Paciente'), (req, res) => {
  res.render('paciente/consultas', {
    layout: 'partials/paciente-layout',
    title: 'Minhas Consultas',
    activeMenu: 'consultas',
    userName: req.session.userName,
  });
});

router.get('/historico', requireRole('Paciente'), (req, res) => {
  res.render('paciente/historico', {
    layout: 'partials/paciente-layout',
    title: 'Histórico',
    activeMenu: 'historico',
    userName: req.session.userName,
  });
});

module.exports = router;

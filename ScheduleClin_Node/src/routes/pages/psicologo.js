const express = require('express');
const { requireRole } = require('../../middleware/auth');

const router = express.Router();

router.get('/', requireRole('Psicologo'), (req, res) => res.redirect('/psicologo/agenda'));

router.get('/agenda', requireRole('Psicologo'), (req, res) => {
  res.render('psicologo/agenda', {
    layout: 'partials/psicologo-layout',
    title: 'Agenda',
    activeMenu: 'agenda',
    userName: req.session.userName,
  });
});

router.get('/queries', requireRole('Psicologo'), (req, res) => {
  res.render('psicologo/queries', {
    layout: 'partials/psicologo-layout',
    title: 'Consultas',
    activeMenu: 'consultas',
    userName: req.session.userName,
  });
});

router.get('/historico', requireRole('Psicologo'), (req, res) => {
  res.render('psicologo/historico', {
    layout: 'partials/psicologo-layout',
    title: 'Histórico',
    activeMenu: 'historico',
    userName: req.session.userName,
  });
});

module.exports = router;

const express = require('express');
const { requireRole } = require('../../middleware/auth');

const router = express.Router();

router.get('/', requireRole('Gestor'), (req, res) => res.redirect('/admin/users'));

router.get('/users', requireRole('Gestor'), (req, res) => {
  res.render('admin/users', {
    layout: 'partials/admin-layout',
    title: 'Usuários',
    activeMenu: 'usuarios',
    userName: req.session.userName,
  });
});

router.get('/calendar', requireRole('Gestor'), (req, res) => {
  res.render('admin/calendar', {
    layout: 'partials/admin-layout',
    title: 'Agenda Geral',
    activeMenu: 'agenda',
    userName: req.session.userName,
  });
});

router.get('/queries', requireRole('Gestor'), (req, res) => {
  res.render('admin/queries', {
    layout: 'partials/admin-layout',
    title: 'Consultas',
    activeMenu: 'consultas',
    userName: req.session.userName,
  });
});

module.exports = router;

const express = require('express');

const router = express.Router();

router.get('/', (req, res) => {
  if (req.session?.userId) {
    const roles = req.session.roles || [];
    if (roles.includes('Gestor')) return res.redirect('/admin/users');
    if (roles.includes('Psicologo')) return res.redirect('/psicologo/agenda');
    if (roles.includes('Paciente')) return res.redirect('/paciente/consultas');
  }
  res.render('home/index', { layout: false, title: 'ScheduleClin' });
});

module.exports = router;

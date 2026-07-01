const express = require('express');
const {
  requireAuth,
  redirectIfAuthenticated,
  requireRole,
  getHomeByRoles,
} = require('../../middleware/auth');
const authService = require('../../services/authService');

const router = express.Router();

router.get('/login', redirectIfAuthenticated, (req, res) => {
  res.render('account/login', {
    layout: false,
    returnUrl: req.query.returnUrl || '',
    errors: [],
  });
});

router.post('/login', redirectIfAuthenticated, async (req, res) => {
  const { email, password, rememberMe, returnUrl } = req.body;
  const errors = [];

  if (!email?.trim()) errors.push('E-mail é obrigatório.');
  if (!password) errors.push('Senha é obrigatória.');

  if (errors.length) {
    return res.render('account/login', { layout: false, returnUrl: returnUrl || '', errors });
  }

  const result = await authService.login(email.trim(), password, req);
  if (!result.ok) {
    return res.render('account/login', { layout: false, returnUrl: returnUrl || '', errors: [result.error] });
  }

  await authService.setSessionUser(req.session, result.user);

  if (result.user.mustChangePassword) {
    return res.redirect('/account/change-password');
  }

  if (returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//')) {
    return res.redirect(returnUrl);
  }

  return res.redirect(getHomeByRoles(result.user.roles));
});

router.post('/logout', requireAuth, async (req, res) => {
  await authService.logout(req.session, req);
  req.session.destroy(() => {
    res.redirect('/account/login');
  });
});

router.get('/change-password', requireAuth, (req, res) => {
  res.render('account/change-password', {
    layout: false,
    errors: [],
    msg: req.query.msg || null,
  });
});

router.post('/change-password', requireAuth, async (req, res) => {
  const { currentPassword, newPassword, confirmPassword } = req.body;
  const errors = [];

  if (!currentPassword) errors.push('Senha atual é obrigatória.');
  if (!newPassword) errors.push('Nova senha é obrigatória.');
  if (newPassword !== confirmPassword) errors.push('Confirmação de senha não confere.');

  if (errors.length) {
    return res.render('account/change-password', { layout: false, errors, msg: null });
  }

  const result = await authService.changePassword(req.session.userId, currentPassword, newPassword);
  if (!result.ok) {
    return res.render('account/change-password', { layout: false, errors: result.errors, msg: null });
  }

  req.session.mustChangePassword = false;
  return res.redirect(getHomeByRoles(req.session.roles));
});

router.get('/access-denied', (req, res) => {
  res.render('account/access-denied', { layout: false });
});

module.exports = router;

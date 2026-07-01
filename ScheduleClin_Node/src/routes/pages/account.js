const express = require('express');
const {
  requireAuth,
  redirectIfAuthenticated,
  getHomeByRoles,
} = require('../../middleware/auth');
const authService = require('../../services/authService');

const router = express.Router();

function isDbConnectionError(err) {
  if (!err) {return false;}
  if (err.code === 'P1001' || err.code === 'P1017') {return true;}
  if (err.name === 'PrismaClientInitializationError') {return true;}
  if (typeof err.message === 'string' && err.message.includes("Can't reach database server")) {return true;}
  return false;
}

const DB_UNAVAILABLE_MSG =
  'Banco de dados indisponível. Verifique se o MySQL/MariaDB está rodando (XAMPP) e se o DATABASE_URL no .env está correto.';

router.get('/login', redirectIfAuthenticated, (req, res) => {
  res.render('account/login', {
    layout: false,
    returnUrl: req.query.returnUrl || '',
    errors: [],
  });
});

router.post('/login', redirectIfAuthenticated, async (req, res, next) => {
  const { email, password, returnUrl } = req.body;
  const errors = [];

  if (!email?.trim()) {errors.push('E-mail é obrigatório.');}
  if (!password) {errors.push('Senha é obrigatória.');}

  if (errors.length) {
    return res.render('account/login', { layout: false, returnUrl: returnUrl || '', errors });
  }

  try {
    const result = await authService.login(email.trim(), password, req);
    if (!result.ok) {
      return res.render('account/login', {
        layout: false,
        returnUrl: returnUrl || '',
        errors: [result.error],
      });
    }

    await authService.setSessionUser(req.session, result.user);

    if (result.user.mustChangePassword) {
      return res.redirect('/account/change-password');
    }

    if (returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//')) {
      return res.redirect(returnUrl);
    }

    return res.redirect(getHomeByRoles(result.user.roles));
  } catch (err) {
    if (isDbConnectionError(err)) {
      return res.render('account/login', {
        layout: false,
        returnUrl: returnUrl || '',
        errors: [DB_UNAVAILABLE_MSG],
      });
    }
    return next(err);
  }
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

router.post('/change-password', requireAuth, async (req, res, next) => {
  const { currentPassword, newPassword, confirmPassword } = req.body;
  const errors = [];

  if (!currentPassword) {errors.push('Senha atual é obrigatória.');}
  if (!newPassword) {errors.push('Nova senha é obrigatória.');}
  if (newPassword !== confirmPassword) {errors.push('Confirmação de senha não confere.');}

  if (errors.length) {
    return res.render('account/change-password', { layout: false, errors, msg: null });
  }

  try {
    const result = await authService.changePassword(req.session.userId, currentPassword, newPassword);
    if (!result.ok) {
      return res.render('account/change-password', { layout: false, errors: result.errors, msg: null });
    }

    req.session.mustChangePassword = false;
    return res.redirect(getHomeByRoles(req.session.roles));
  } catch (err) {
    if (isDbConnectionError(err)) {
      return res.render('account/change-password', {
        layout: false,
        errors: [DB_UNAVAILABLE_MSG],
        msg: null,
      });
    }
    return next(err);
  }
});

router.get('/access-denied', (req, res) => {
  res.render('account/access-denied', { layout: false });
});

module.exports = router;

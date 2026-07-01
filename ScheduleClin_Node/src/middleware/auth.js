const { runWithAuditContext } = require('./audit');

function attachAuditContext(req, res, next) {
  const ctx = {
    userId: req.session?.userId || null,
    userName: req.session?.userName || null,
    ipAddress: req.ip || req.connection?.remoteAddress || null,
    skipAudit: false,
  };
  runWithAuditContext(ctx, () => next());
}

function requireAuth(req, res, next) {
  if (req.session?.userId) return next();

  if (req.path.startsWith('/api') || req.originalUrl.startsWith('/api') || req.originalUrl.startsWith('/get-users')) {
    return res.status(401).json({ message: 'Não autenticado.' });
  }
  return res.redirect('/account/login');
}

function requireRole(...roles) {
  return (req, res, next) => {
    if (!req.session?.userId) {
      if (req.path.startsWith('/api') || req.originalUrl.startsWith('/api')) {
        return res.status(401).json({ message: 'Não autenticado.' });
      }
      return res.redirect('/account/login');
    }

    const userRoles = req.session.roles || [];
    if (roles.some((r) => userRoles.includes(r))) return next();

    if (req.path.startsWith('/api') || req.originalUrl.startsWith('/api')) {
      return res.status(403).json({ message: 'Acesso negado.' });
    }
    return res.redirect('/account/access-denied');
  };
}

function redirectIfAuthenticated(req, res, next) {
  if (req.session?.userId && !req.session?.mustChangePassword) {
    return res.redirect(getHomeByRoles(req.session.roles || []));
  }
  next();
}

function getHomeByRoles(roles) {
  if (roles.includes('Gestor')) return '/admin/users';
  if (roles.includes('Psicologo')) return '/psicologo/agenda';
  if (roles.includes('Paciente')) return '/paciente/consultas';
  return '/';
}

module.exports = {
  attachAuditContext,
  requireAuth,
  requireRole,
  redirectIfAuthenticated,
  getHomeByRoles,
};

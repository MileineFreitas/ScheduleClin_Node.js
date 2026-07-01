const { doubleCsrf } = require('csrf-csrf');
const env = require('../config/env');

const {
  generateToken,
  doubleCsrfProtection,
} = doubleCsrf({
  getSecret: () => env.sessionSecret,
  getSessionIdentifier: (req) => req.sessionID || 'anonymous',
  cookieName: '_csrf',
  cookieOptions: {
    httpOnly: true,
    secure: env.cookieSecure,
    sameSite: 'lax',
  },
  size: 64,
  ignoredMethods: ['GET', 'HEAD', 'OPTIONS'],
  getTokenFromRequest: (req) => req.headers['x-csrf-token'] || req.body?._csrf,
});

function csrfTokenMiddleware(req, res, next) {
  if (req.session) {
    req.session._csrfInit = true;
  }
  try {
    // validateOnReuse=false evita loop quando o cookie _csrf está desatualizado
    res.locals.csrfToken = generateToken(req, res, false, false);
  } catch (err) {
    if (err.code === 'EBADCSRFTOKEN') {
      res.clearCookie('_csrf');
      res.locals.csrfToken = generateToken(req, res, true, false);
      return next();
    }
    return next(err);
  }
  next();
}

function csrfProtection(req, res, next) {
  return doubleCsrfProtection(req, res, next);
}

module.exports = {
  csrfTokenMiddleware,
  csrfProtection,
  generateCsrfToken: generateToken,
};

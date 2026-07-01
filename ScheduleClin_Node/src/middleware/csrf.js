const { doubleCsrf } = require('csrf-csrf');
const env = require('../config/env');

const {
  generateCsrfToken,
  doubleCsrfProtection,
} = doubleCsrf({
  getSecret: () => env.sessionSecret,
  getSessionIdentifier: (req) => req.sessionID || 'anonymous',
  cookieName: '_csrf',
  cookieOptions: {
    httpOnly: true,
    secure: env.cookieSecure,
    sameSite: 'strict',
  },
  size: 64,
  ignoredMethods: ['GET', 'HEAD', 'OPTIONS'],
  getCsrfTokenFromRequest: (req) => req.headers['x-csrf-token'] || req.body?._csrf,
});

function csrfTokenMiddleware(req, res, next) {
  res.locals.csrfToken = generateCsrfToken(req, res);
  next();
}

function csrfProtection(req, res, next) {
  if (['GET', 'HEAD', 'OPTIONS'].includes(req.method)) return next();
  return doubleCsrfProtection(req, res, next);
}

module.exports = { csrfTokenMiddleware, csrfProtection, generateCsrfToken };

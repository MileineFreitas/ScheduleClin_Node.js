require('dotenv').config();

module.exports = {
  port: parseInt(process.env.PORT || '3000', 10),
  nodeEnv: process.env.NODE_ENV || 'development',
  sessionSecret: process.env.SESSION_SECRET || 'dev-secret-change-in-production',
  cookieSecure: process.env.COOKIE_SECURE === 'true',
  isDev: (process.env.NODE_ENV || 'development') !== 'production',
};

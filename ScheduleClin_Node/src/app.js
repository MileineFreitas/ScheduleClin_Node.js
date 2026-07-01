const path = require('path');
const express = require('express');
const session = require('express-session');
const expressLayouts = require('express-ejs-layouts');
const swaggerUi = require('swagger-ui-express');
const swaggerJsdoc = require('swagger-jsdoc');
const env = require('./config/env');
const { attachAuditContext, requireRole } = require('./middleware/auth');
const { csrfTokenMiddleware, csrfProtection } = require('./middleware/csrf');
const userService = require('./services/userService');

const accountPages = require('./routes/pages/account');
const adminPages = require('./routes/pages/admin');
const psicologoPages = require('./routes/pages/psicologo');
const pacientePages = require('./routes/pages/paciente');
const homePages = require('./routes/pages/home');

const userApi = require('./routes/api/user');
const calendarApi = require('./routes/api/calendar');
const psicologoAgendaApi = require('./routes/api/psicologoAgenda');
const pacienteAgendaApi = require('./routes/api/pacienteAgenda');

function createApp() {
  const app = express();

  app.set('trust proxy', 1);
  app.set('view engine', 'ejs');
  app.set('views', path.join(__dirname, '..', 'views'));
  app.use(expressLayouts);
  app.set('layout extractScripts', true);
  app.set('layout extractStyles', true);

  app.use(express.json());
  app.use(express.urlencoded({ extended: true }));
  app.use(express.static(path.join(__dirname, '..', 'public')));

  app.use(session({
    secret: env.sessionSecret,
    resave: false,
    saveUninitialized: false,
    rolling: true,
    cookie: {
      maxAge: 30 * 60 * 1000,
      httpOnly: true,
      secure: env.cookieSecure,
      sameSite: 'lax',
    },
  }));

  app.use(attachAuditContext);
  app.use(csrfTokenMiddleware);
  app.use(csrfProtection);

  app.use((req, res, next) => {
    res.locals.userName = req.session?.userName || '';
    res.locals.csrfToken = res.locals.csrfToken || '';
    next();
  });

  if (env.isDev) {
    const swaggerSpec = swaggerJsdoc({
      definition: {
        openapi: '3.0.0',
        info: {
          title: 'ScheduleClin API',
          version: 'v1',
          description: 'API para gestão de agendamentos, usuários e perfis',
        },
      },
      apis: [path.join(__dirname, 'routes', 'api', '*.js')],
    });
    app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));
  }

  app.use('/', homePages);
  app.use('/account', accountPages);
  app.use('/admin', adminPages);
  app.use('/psicologo', psicologoPages);
  app.use('/paciente', pacientePages);

  app.get('/get-users', requireRole('Gestor'), async (req, res) => {
    try {
      res.json(await userService.getUsers());
    } catch (err) {
      console.error(err);
      res.status(500).json({ message: 'Erro interno.' });
    }
  });

  app.use('/api/User', userApi);
  app.use('/api/Calendar', calendarApi);
  app.use('/api/PsicologoAgenda', psicologoAgendaApi);
  app.use('/api/PacienteAgenda', pacienteAgendaApi);

  app.use((err, req, res, next) => {
    if (err.code === 'EBADCSRFTOKEN') {
      if (req.path.startsWith('/api') || req.originalUrl.startsWith('/api')) {
        return res.status(403).json({ message: 'Token CSRF inválido.' });
      }
      return res.status(403).send('Token CSRF inválido.');
    }
    console.error(err);
    res.status(500).send('Erro interno.');
  });

  return app;
}

module.exports = { createApp };

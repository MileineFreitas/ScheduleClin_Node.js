const { createApp } = require('./app');
const { seed } = require('./data/seed');
const env = require('./config/env');

async function main() {
  try {
    await seed();
  } catch (err) {
    console.warn('Seed ignorado ou falhou (verifique DATABASE_URL):', err.message);
  }

  const app = createApp();
  app.listen(env.port, () => {
    console.log(`ScheduleClin Node rodando em http://localhost:${env.port}`);
    if (env.isDev) console.log(`Swagger: http://localhost:${env.port}/api-docs`);
  });
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});

const { createApp } = require('./app');
const { seed } = require('./data/seed');
const env = require('./config/env');

async function main() {
  try {
    await seed();
  } catch (err) {
    console.warn('');
    console.warn('⚠ Seed ignorado — banco de dados não acessível.');
    console.warn('  Verifique se o MySQL/MariaDB está rodando (XAMPP) e configure o arquivo .env');
    console.warn('  (copie .env.example → .env e ajuste DATABASE_URL).');
    console.warn(`  Detalhe: ${err.message.split('\n')[0]}`);
    console.warn('');
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

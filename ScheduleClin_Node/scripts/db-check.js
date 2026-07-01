/**
 * Verifica conexão com MariaDB/MySQL (DATABASE_URL no .env).
 * Uso: node scripts/db-check.js
 */
require('dotenv').config({ override: true });
const { PrismaClient } = require('@prisma/client');

const url = process.env.DATABASE_URL || '';
const masked = url.replace(/:([^:@/]+)@/, ':***@');

async function main() {
  if (!url) {
    console.error('DATABASE_URL não definido. Copie .env.example para .env e configure a conexão.');
    process.exit(1);
  }

  console.log(`Testando: ${masked}\n`);

  const prisma = new PrismaClient();
  try {
    await prisma.$queryRaw`SELECT 1 AS ok`;
    console.log('Conexão OK — MariaDB/MySQL acessível.');
  } catch (err) {
    console.error('Falha na conexão:\n');
    console.error(err.message?.split('\n').slice(0, 5).join('\n'));
    console.error('\n--- Como corrigir (XAMPP / MariaDB) ---');
    console.error('1. Abra o XAMPP Control Panel e inicie o módulo MySQL.');
    console.error('2. No phpMyAdmin (http://localhost/phpmyadmin), crie o banco "scheduleclin".');
    console.error('3. Ajuste DATABASE_URL no .env:');
    console.error('   mysql://root:@localhost:3306/scheduleclin   (senha vazia)');
    console.error('   mysql://root:SUA_SENHA@localhost:3306/scheduleclin');
    console.error('4. Rode: npm run prisma:push && npm run dev');
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

main();

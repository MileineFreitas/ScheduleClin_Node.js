const { PrismaClient } = require('@prisma/client');
const { createAuditExtension } = require('../middleware/audit');

let prisma;

function getPrisma() {
  if (!prisma) {
    const base = new PrismaClient();
    prisma = base.$extends(createAuditExtension(base));
  }
  return prisma;
}

module.exports = { getPrisma };

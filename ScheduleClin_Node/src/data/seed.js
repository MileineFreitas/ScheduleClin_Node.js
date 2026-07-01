const { randomUUID } = require('crypto');
const { getPrisma } = require('../utils/prisma');
const { ProfileIds, Perfis } = require('../utils/perfis');
const {
  hashPassword,
  normalize,
  newSecurityStamp,
  newConcurrencyStamp,
} = require('../utils/password');

async function seedProfiles(prisma) {
  const perfis = [
    { profileId: ProfileIds.Gestor, name: Perfis.Gestor, description: 'Administrador do sistema' },
    { profileId: ProfileIds.Psicologo, name: Perfis.Psicologo, description: 'Funcionário — psicólogo(a)' },
    { profileId: ProfileIds.Paciente, name: Perfis.Paciente, description: 'Paciente da clínica' },
  ];

  for (const perfil of perfis) {
    const exists = await prisma.profile.findUnique({ where: { profileId: perfil.profileId } });
    if (!exists) {await prisma.profile.create({ data: perfil });}
  }
}

async function seedRoles(prisma) {
  for (const papel of Perfis.Todos) {
    const exists = await prisma.role.findFirst({ where: { normalizedName: normalize(papel) } });
    if (!exists) {
      await prisma.role.create({
        data: {
          id: randomUUID(),
          name: papel,
          normalizedName: normalize(papel),
          concurrencyStamp: newConcurrencyStamp(),
        },
      });
    }
  }
}

async function seedGestor(prisma) {
  const emailGestor = 'gestor@scheduleclin.local';
  let gestor = await prisma.user.findFirst({ where: { normalizedEmail: normalize(emailGestor) } });

  const gestorRole = await prisma.role.findFirst({ where: { normalizedName: normalize(Perfis.Gestor) } });

  if (gestor) {
    if (!gestor.perfilId) {
      await prisma.user.update({ where: { id: gestor.id }, data: { perfilId: ProfileIds.Gestor } });
    }
    if (gestorRole) {
      const hasRole = await prisma.userRole.findUnique({
        where: { userId_roleId: { userId: gestor.id, roleId: gestorRole.id } },
      });
      if (!hasRole) {await prisma.userRole.create({ data: { userId: gestor.id, roleId: gestorRole.id } });}
    }
    return;
  }

  gestor = await prisma.user.create({
    data: {
      id: randomUUID(),
      userName: emailGestor,
      normalizedUserName: normalize(emailGestor),
      email: emailGestor,
      normalizedEmail: normalize(emailGestor),
      emailConfirmed: true,
      passwordHash: hashPassword('Gestor@123'),
      securityStamp: newSecurityStamp(),
      concurrencyStamp: newConcurrencyStamp(),
      perfilId: ProfileIds.Gestor,
      dataNascimento: new Date('1990-01-01'),
      mustChangePassword: true,
      active: true,
    },
  });

  if (gestorRole) {
    await prisma.userRole.create({ data: { userId: gestor.id, roleId: gestorRole.id } });
  }
}

async function syncExistingUsers(prisma) {
  const users = await prisma.user.findMany({ include: { perfil: true } });
  const profiles = await prisma.profile.findMany();

  for (const user of users) {
    const userRoles = await prisma.userRole.findMany({
      where: { userId: user.id },
      include: { role: true },
    });
    const roles = userRoles.map((ur) => ur.role?.name).filter(Boolean);

    if (roles.length > 0 && !user.perfilId) {
      const profile = profiles.find((p) => p.name === roles[0]);
      if (profile) {
        await prisma.user.update({ where: { id: user.id }, data: { perfilId: profile.profileId } });
      }
    } else if (user.perfilId && roles.length === 0 && user.perfil?.name) {
      const role = await prisma.role.findFirst({ where: { normalizedName: normalize(user.perfil.name) } });
      if (role) {await prisma.userRole.create({ data: { userId: user.id, roleId: role.id } });}
    }
  }
}

async function seed() {
  const prisma = getPrisma();
  const { runWithAuditContext } = require('../middleware/audit');
  await runWithAuditContext({ skipAudit: true }, async () => {
    await seedProfiles(prisma);
    await seedRoles(prisma);
    await seedGestor(prisma);
    await syncExistingUsers(prisma);
  });
  console.log('Seed concluído.');
}

module.exports = { seed };

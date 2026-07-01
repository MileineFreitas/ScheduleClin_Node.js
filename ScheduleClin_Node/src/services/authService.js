const { getPrisma } = require('../utils/prisma');
const {
  verifyPassword,
  hashPassword,
  validatePasswordPolicy,
  normalize,
  newSecurityStamp,
  newConcurrencyStamp,
} = require('../utils/password');
const { registrarAudit } = require('../middleware/audit');

const MAX_FAILED = 5;
const LOCKOUT_MINUTES = 15;

async function loadUserRoles(prisma, userId) {
  const userRoles = await prisma.userRole.findMany({
    where: { userId },
    include: { role: true },
  });
  return userRoles.map((ur) => ur.role?.name).filter(Boolean);
}

async function login(email, password, req) {
  const prisma = getPrisma();
  const ip = req.ip || req.connection?.remoteAddress;

  const user = await prisma.user.findFirst({
    where: { normalizedEmail: normalize(email) },
    include: { perfil: true },
  });

  if (!user || !user.active) {
    await registrarAudit(prisma, {
      action: 'LoginFalhou',
      userId: user?.id,
      userName: email,
      details: 'Usuário inexistente ou inativo',
      ipAddress: ip,
    });
    return { ok: false, error: 'Credenciais inválidas.' };
  }

  if (user.lockoutEnabled && user.lockoutEnd && new Date(user.lockoutEnd) > new Date()) {
    await registrarAudit(prisma, {
      action: 'LoginBloqueado',
      userId: user.id,
      userName: user.email,
      details: 'Conta bloqueada por tentativas',
      ipAddress: ip,
    });
    return { ok: false, error: 'Conta bloqueada temporariamente por excesso de tentativas.' };
  }

  if (!verifyPassword(user.passwordHash, password)) {
    const failed = user.accessFailedCount + 1;
    const data = { accessFailedCount: failed };
    if (failed >= MAX_FAILED) {
      data.lockoutEnd = new Date(Date.now() + LOCKOUT_MINUTES * 60 * 1000);
    }
    await prisma.user.update({ where: { id: user.id }, data });

    await registrarAudit(prisma, {
      action: 'LoginFalhou',
      userId: user.id,
      userName: user.email,
      details: 'Senha incorreta',
      ipAddress: ip,
    });
    return { ok: false, error: 'Credenciais inválidas.' };
  }

  await prisma.user.update({
    where: { id: user.id },
    data: { accessFailedCount: 0, lockoutEnd: null },
  });

  const roles = await loadUserRoles(prisma, user.id);

  await registrarAudit(prisma, {
    action: 'Login',
    userId: user.id,
    userName: user.email,
    ipAddress: ip,
  });

  return {
    ok: true,
    user: {
      id: user.id,
      userName: user.userName,
      email: user.email,
      mustChangePassword: user.mustChangePassword,
      roles,
    },
  };
}

async function logout(session, req) {
  const prisma = getPrisma();
  if (session?.userId) {
    await registrarAudit(prisma, {
      action: 'Logout',
      userId: session.userId,
      userName: session.userName,
      ipAddress: req.ip,
    });
  }
}

async function changePassword(userId, currentPassword, newPassword) {
  const prisma = getPrisma();
  const user = await prisma.user.findUnique({ where: { id: userId } });
  if (!user) {return { ok: false, error: 'Usuário não encontrado.' };}

  if (!verifyPassword(user.passwordHash, currentPassword)) {
    return { ok: false, errors: ['Senha atual incorreta.'] };
  }

  const policyErrors = validatePasswordPolicy(newPassword);
  if (policyErrors.length) {return { ok: false, errors: policyErrors };}

  await prisma.user.update({
    where: { id: userId },
    data: {
      passwordHash: hashPassword(newPassword),
      securityStamp: newSecurityStamp(),
      concurrencyStamp: newConcurrencyStamp(),
      mustChangePassword: false,
    },
  });

  return { ok: true };
}

async function setSessionUser(session, user) {
  session.userId = user.id;
  session.userName = user.userName;
  session.email = user.email;
  session.mustChangePassword = user.mustChangePassword;
  session.roles = user.roles;
}

module.exports = {
  login,
  logout,
  changePassword,
  setSessionUser,
  loadUserRoles,
};

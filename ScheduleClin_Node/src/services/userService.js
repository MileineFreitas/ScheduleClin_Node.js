const { randomUUID } = require('crypto');
const { getPrisma } = require('../utils/prisma');
const { Perfis } = require('../utils/perfis');
const {
  hashPassword,
  generateProvisionalPassword,
  normalize,
  newSecurityStamp,
  newConcurrencyStamp,
} = require('../utils/password');

async function getUsers() {
  const prisma = getPrisma();
  const users = await prisma.user.findMany({ include: { perfil: true } });
  const userRoles = await prisma.userRole.findMany({ include: { role: true } });

  const roleByUser = {};
  for (const ur of userRoles) {
    if (ur.role?.name) roleByUser[ur.userId] = ur.role.name;
  }

  return users.map((u) => ({
    id: u.id,
    userName: u.userName,
    email: u.email,
    cpf: u.cpf,
    crp: u.crp,
    perfilId: u.perfilId,
    role: roleByUser[u.id] || u.perfil?.name,
    perfilNome: u.perfil?.name || roleByUser[u.id],
    dataNascimento: u.dataNascimento,
    mustChangePassword: u.mustChangePassword,
    active: u.active,
  }));
}

async function getProfiles() {
  const prisma = getPrisma();
  return prisma.profile.findMany({
    where: { name: { in: Perfis.Cadastro } },
    select: { profileId: true, name: true, description: true },
  }).then((rows) => rows.map((p) => ({
    profileId: p.profileId,
    name: p.name,
    description: p.description,
  })));
}

async function createUser(dto) {
  const prisma = getPrisma();
  const perfil = await prisma.profile.findFirst({ where: { profileId: dto.perfilId } });

  if (!perfil || !Perfis.Cadastro.includes(perfil.name)) {
    return { ok: false, status: 400, message: 'Perfil inválido. Use Psicologo ou Paciente.' };
  }

  if (perfil.name === Perfis.Psicologo && !dto.crp?.trim()) {
    return { ok: false, status: 400, message: 'CRP é obrigatório para psicólogo(a).' };
  }

  if (dto.cpf && await prisma.user.findFirst({ where: { cpf: dto.cpf } })) {
    return { ok: false, status: 400, message: 'Já existe um usuário cadastrado com esse CPF.' };
  }

  const crpTrim = dto.crp?.trim();
  if (crpTrim && await prisma.user.findFirst({ where: { crp: crpTrim } })) {
    return { ok: false, status: 400, message: 'Já existe um usuário cadastrado com esse CRP.' };
  }

  const senhaProvisoria = generateProvisionalPassword();
  const userId = randomUUID();
  const role = await prisma.role.findFirst({ where: { normalizedName: normalize(perfil.name) } });

  const user = await prisma.user.create({
    data: {
      id: userId,
      userName: dto.userName,
      normalizedUserName: normalize(dto.userName),
      email: dto.email,
      normalizedEmail: normalize(dto.email),
      emailConfirmed: true,
      passwordHash: hashPassword(senhaProvisoria),
      securityStamp: newSecurityStamp(),
      concurrencyStamp: newConcurrencyStamp(),
      cpf: dto.cpf,
      crp: perfil.name === Perfis.Psicologo ? crpTrim : null,
      perfilId: dto.perfilId,
      dataNascimento: new Date(dto.dataNascimento),
      mustChangePassword: true,
      active: true,
    },
  });

  if (role) {
    await prisma.userRole.create({ data: { userId: user.id, roleId: role.id } });
  }

  return {
    ok: true,
    status: 201,
    body: {
      id: user.id,
      userName: user.userName,
      email: user.email,
      role: perfil.name,
      senhaProvisoria,
    },
  };
}

async function updateUser(id, dto) {
  const prisma = getPrisma();
  const user = await prisma.user.findUnique({ where: { id } });
  if (!user) return { ok: false, status: 404, message: 'Usuário não encontrado.' };

  const data = {};

  if (dto.perfilId) {
    const perfil = await prisma.profile.findFirst({ where: { profileId: dto.perfilId } });
    if (!perfil || !Perfis.Cadastro.includes(perfil.name)) {
      return { ok: false, status: 400, message: 'Perfil inválido.' };
    }
    data.perfilId = dto.perfilId;

    await prisma.userRole.deleteMany({ where: { userId: id } });
    const role = await prisma.role.findFirst({ where: { normalizedName: normalize(perfil.name) } });
    if (role) await prisma.userRole.create({ data: { userId: id, roleId: role.id } });
  }

  if (dto.userName) {
    data.userName = dto.userName;
    data.normalizedUserName = normalize(dto.userName);
  }
  if (dto.email) {
    data.email = dto.email;
    data.normalizedEmail = normalize(dto.email);
  }
  if (dto.cpf !== undefined) {
    if (dto.cpf && await prisma.user.findFirst({ where: { cpf: dto.cpf, NOT: { id } } })) {
      return { ok: false, status: 400, message: 'Já existe um usuário cadastrado com esse CPF.' };
    }
    data.cpf = dto.cpf;
  }
  if (dto.dataNascimento) data.dataNascimento = new Date(dto.dataNascimento);
  if (dto.crp !== undefined) {
    const crp = dto.crp?.trim() || null;
    if (crp && await prisma.user.findFirst({ where: { crp, NOT: { id } } })) {
      return { ok: false, status: 400, message: 'Já existe um usuário cadastrado com esse CRP.' };
    }
    data.crp = crp;
  }

  await prisma.user.update({ where: { id }, data });
  return { ok: true, status: 204 };
}

async function alterarStatus(id, isActive) {
  const prisma = getPrisma();
  const user = await prisma.user.findUnique({ where: { id } });
  if (!user) return { ok: false, status: 400, message: 'Usuário não encontrado.' };

  await prisma.user.update({ where: { id }, data: { active: isActive } });
  return { ok: true, body: { id, active: isActive } };
}

async function resetPassword(id) {
  const prisma = getPrisma();
  const user = await prisma.user.findUnique({ where: { id } });
  if (!user) return { ok: false, status: 404, message: 'Usuário não encontrado.' };

  const senhaProvisoria = generateProvisionalPassword();
  await prisma.user.update({
    where: { id },
    data: {
      passwordHash: hashPassword(senhaProvisoria),
      securityStamp: newSecurityStamp(),
      mustChangePassword: true,
    },
  });

  return { ok: true, body: { id, senhaProvisoria } };
}

module.exports = {
  getUsers,
  getProfiles,
  createUser,
  updateUser,
  alterarStatus,
  resetPassword,
};

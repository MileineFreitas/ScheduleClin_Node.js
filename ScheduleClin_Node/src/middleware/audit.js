const { AsyncLocalStorage } = require('async_hooks');

const auditContext = new AsyncLocalStorage();

const RUIDO = new Set([
  'passwordHash', 'securityStamp', 'concurrencyStamp',
  'accessFailedCount', 'lockoutEnd', 'lockoutEnabled',
  'normalizedUserName', 'normalizedEmail',
]);

function runWithAuditContext(context, fn) {
  return auditContext.run(context, fn);
}

function getAuditContext() {
  return auditContext.getStore();
}

function montarDetalhes(before, after) {
  if (!before || !after) {return null;}
  const parts = [];
  for (const key of Object.keys(after)) {
    if (RUIDO.has(key)) {continue;}
    if (before[key] instanceof Date && after[key] instanceof Date) {
      if (before[key].getTime() !== after[key].getTime()) {parts.push(`${key}=${after[key].toISOString()}`);}
    } else if (String(before[key]) !== String(after[key])) {
      parts.push(`${key}=${after[key]}`);
    }
  }
  return parts.length > 0 ? parts.join('; ') : null;
}

function createAuditExtension(baseClient) {
  return {
    name: 'audit',
    query: {
      async $allOperations({ model, operation, args, query }) {
        const auditable = model === 'User' || model === 'Calendar';
        const ctx = getAuditContext();

        if (!auditable || !ctx || operation === 'findMany' || operation === 'findFirst' || operation === 'findUnique' || operation === 'count') {
          return query(args);
        }

        const modelClient = { User: 'user', Calendar: 'calendar' }[model];
        if (!modelClient) {return query(args);}

        let before = null;
        if (operation === 'update' || operation === 'delete') {
          const idField = model === 'User' ? 'id' : 'calendarID';
          const id = args.where?.[idField];
          if (id) {
            before = await baseClient[modelClient].findUnique({ where: { [idField]: id } });
          }
        }

        const result = await query(args);

        if (ctx.skipAudit) {return result;}

        let action;
        let entityId;
        let details = null;

        if (operation === 'create') {
          action = `Added ${model}`;
          entityId = model === 'User' ? result.id : result.calendarID;
        } else if (operation === 'update') {
          details = montarDetalhes(before, result);
          if (!details) {return result;}
          action = `Modified ${model}`;
          entityId = model === 'User' ? result.id : result.calendarID;
        } else if (operation === 'delete') {
          action = `Deleted ${model}`;
          entityId = model === 'User' ? before?.id : before?.calendarID;
        } else {
          return result;
        }

        await baseClient.auditLog.create({
          data: {
            userId: ctx.userId || null,
            userName: ctx.userName || null,
            action,
            entity: model,
            entityId: entityId ? String(entityId) : null,
            details,
            ipAddress: ctx.ipAddress || null,
          },
        });

        return result;
      },
    },
  };
}

async function registrarAudit(prisma, { action, userId, userName, details, ipAddress, entity, entityId }) {
  await prisma.auditLog.create({
    data: {
      action,
      userId: userId || null,
      userName: userName || null,
      details: details || null,
      ipAddress: ipAddress || null,
      entity: entity || null,
      entityId: entityId || null,
    },
  });
}

module.exports = {
  runWithAuditContext,
  getAuditContext,
  createAuditExtension,
  registrarAudit,
};

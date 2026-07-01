const crypto = require('crypto');

const ITERATIONS = 10000;
const SUBKEY_LENGTH = 32;
const SALT_LENGTH = 16;

function hashPassword(password) {
  const salt = crypto.randomBytes(SALT_LENGTH);
  const subkey = crypto.pbkdf2Sync(password, salt, ITERATIONS, SUBKEY_LENGTH, 'sha256');

  const output = Buffer.alloc(13 + salt.length + 4 + subkey.length);
  output[0] = 0x01;
  output.writeUInt32BE(2, 1);
  output.writeUInt32BE(ITERATIONS, 5);
  output.writeUInt32BE(salt.length, 9);
  salt.copy(output, 13);
  output.writeUInt32BE(subkey.length, 13 + salt.length);
  subkey.copy(output, 17 + salt.length);

  return output.toString('base64');
}

function verifyV3(decoded, password) {
  if (decoded.length < 13) return false;

  const prf = decoded.readUInt32BE(1);
  const iterations = decoded.readUInt32BE(5);
  const saltLength = decoded.readUInt32BE(9);

  if (13 + saltLength + 4 > decoded.length) return false;

  const salt = decoded.subarray(13, 13 + saltLength);
  const subkeyLength = decoded.readUInt32BE(13 + saltLength);

  if (17 + saltLength + subkeyLength > decoded.length) return false;

  const expectedSubkey = decoded.subarray(17 + saltLength, 17 + saltLength + subkeyLength);
  const algorithm = prf === 2 ? 'sha256' : prf === 1 ? 'sha1' : null;
  if (!algorithm) return false;

  const actualSubkey = crypto.pbkdf2Sync(password, salt, iterations, subkeyLength, algorithm);
  if (actualSubkey.length !== expectedSubkey.length) return false;
  return crypto.timingSafeEqual(actualSubkey, expectedSubkey);
}

function verifyPassword(hashedPassword, password) {
  if (!hashedPassword) return false;

  try {
    const decoded = Buffer.from(hashedPassword, 'base64');
    if (decoded[0] === 0x01) return verifyV3(decoded, password);

    // ASP.NET Identity v2 (0x00 prefix) or legacy PBKDF2
    if (decoded[0] === 0x00) {
      return verifyV3(decoded.subarray(1), password);
    }

    // Legacy format: salt + subkey without header
    if (decoded.length === SALT_LENGTH + SUBKEY_LENGTH) {
      const salt = decoded.subarray(0, SALT_LENGTH);
      const expected = decoded.subarray(SALT_LENGTH);
      const actual = crypto.pbkdf2Sync(password, salt, 1000, SUBKEY_LENGTH, 'sha1');
      return crypto.timingSafeEqual(actual, expected);
    }
  } catch {
    return false;
  }

  return false;
}

function validatePasswordPolicy(password) {
  const errors = [];
  if (!password || password.length < 8) errors.push('A senha deve ter pelo menos 8 caracteres.');
  if (!/[A-Z]/.test(password)) errors.push('A senha deve conter ao menos uma letra maiúscula.');
  if (!/[a-z]/.test(password)) errors.push('A senha deve conter ao menos uma letra minúscula.');
  if (!/[0-9]/.test(password)) errors.push('A senha deve conter ao menos um dígito.');
  return errors;
}

function generateProvisionalPassword() {
  return `Prov@${crypto.randomBytes(4).toString('hex')}`;
}

function normalize(value) {
  return value ? value.toUpperCase() : null;
}

function newSecurityStamp() {
  return crypto.randomUUID();
}

function newConcurrencyStamp() {
  return crypto.randomUUID();
}

module.exports = {
  hashPassword,
  verifyPassword,
  validatePasswordPolicy,
  generateProvisionalPassword,
  normalize,
  newSecurityStamp,
  newConcurrencyStamp,
};

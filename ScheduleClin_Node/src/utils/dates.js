/**
 * Converte data BR (dd/mm/aaaa) ou ISO (aaaa-mm-dd) em Date local.
 * Retorna null se inválida.
 */
function parseDataNascimento(value) {
  if (value === null || value === undefined || value === '') {
    return null;
  }

  const str = String(value).trim();

  const br = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(str);
  if (br) {
    const dia = Number(br[1]);
    const mes = Number(br[2]);
    const ano = Number(br[3]);
    const date = new Date(ano, mes - 1, dia);
    if (
      date.getFullYear() === ano
      && date.getMonth() === mes - 1
      && date.getDate() === dia
    ) {return date;}
    return null;
  }

  const iso = /^(\d{4})-(\d{2})-(\d{2})/.exec(str);
  if (iso) {
    const ano = Number(iso[1]);
    const mes = Number(iso[2]);
    const dia = Number(iso[3]);
    const date = new Date(ano, mes - 1, dia);
    if (
      date.getFullYear() === ano
      && date.getMonth() === mes - 1
      && date.getDate() === dia
    ) {return date;}
    return null;
  }

  const parsed = new Date(str);
  if (Number.isNaN(parsed.getTime())) {return null;}
  return parsed;
}

module.exports = { parseDataNascimento };

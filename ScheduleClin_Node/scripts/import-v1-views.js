const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const ROOT = path.join(__dirname, '..');
const REF = 'e8d0c7e^';

function gitShow(gitPath) {
  return execSync(`git -C "${path.join(ROOT, '..')}" show "${REF}:${gitPath}"`, {
    encoding: 'utf8',
    maxBuffer: 10 * 1024 * 1024,
  });
}

function cshtmlToEjs(content, opts = {}) {
  let s = content;

  // Remove Razor header block
  s = s.replace(/^@\{[\s\S]*?\}\s*\n?/m, '');

  // Partials
  s = s.replace(
    /@await Html\.PartialAsync\("_ModalQueries"\)/g,
    "<%- include('../partials/modal-queries') %>",
  );
  s = s.replace(
    /@await Html\.PartialAsync\("_ModalNovaConsulta"\)/g,
    "<%- include('../partials/modal-nova-consulta') %>",
  );

  // Section Scripts -> plain script block
  s = s.replace(/@section Scripts \{\s*\n?/g, '');
  s = s.replace(/\n?\}\s*$/g, '');

  // Static paths
  s = s.replace(/~\//g, '/');
  s = s.replace(/asp-append-version="true"\s*/g, '');

  if (opts.partial) {
    s = s.replace(/<partial name="_CsrfFetch"\s*\/>/g, "<%- include('csrf-fetch') %>");
  }

  return s.trim() + '\n';
}

const jobs = [
  {
    src: 'ScheduleClin_V1/Views/Admin/_ModalQueries.cshtml',
    dest: 'views/partials/modal-queries.ejs',
    partial: true,
  },
  {
    src: 'ScheduleClin_V1/Views/Psicologo/_ModalNovaConsulta.cshtml',
    dest: 'views/partials/modal-nova-consulta.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Admin/Calendar.cshtml',
    dest: 'views/admin/calendar.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Admin/Queries.cshtml',
    dest: 'views/admin/queries.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Psicologo/Agenda.cshtml',
    dest: 'views/psicologo/agenda.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Psicologo/Queries.cshtml',
    dest: 'views/psicologo/queries.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Psicologo/Historico.cshtml',
    dest: 'views/psicologo/historico.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Paciente/Consultas.cshtml',
    dest: 'views/paciente/consultas.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Paciente/Historico.cshtml',
    dest: 'views/paciente/historico.ejs',
  },
  {
    src: 'ScheduleClin_V1/Views/Admin/Users.cshtml',
    dest: 'views/admin/users.ejs',
  },
];

for (const job of jobs) {
  const raw = gitShow(job.src);
  const out = cshtmlToEjs(raw, job);
  const destPath = path.join(ROOT, job.dest);
  fs.mkdirSync(path.dirname(destPath), { recursive: true });
  fs.writeFileSync(destPath, out, 'utf8');
  console.log('Wrote', job.dest);
}

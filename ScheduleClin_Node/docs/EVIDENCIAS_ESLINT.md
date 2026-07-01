# Evidências ESLint — Onde tirar print

Pasta: `ScheduleClin_Node/docs/`

---

## Print 1 — Instalação

**Onde:** PowerShell após instalar (já feito no projeto).

**Comando (se quiser repetir o print):**
```powershell
cd C:\Users\Acer\Documents\GitHub\ScheduleClin_Node.js\ScheduleClin_Node
npm install eslint @eslint/js --save-dev
```

**O que mostrar:** linhas `added X packages` e `found 0 vulnerabilities`.

---

## Print 2 — Configuração

**Onde:** VS Code com o arquivo aberto:

```
ScheduleClin_Node/eslint.config.js
```

**O que mostrar:** regras (`curly`, `eqeqeq`, `semi`, etc.) e `ignores`.

**Extra (opcional):** `ScheduleClin_Node/package.json` — scripts `"lint"` e `"lint:fix"`.

---

## Print 3 — Execução ANTES (obrigatório)

**Opção A — Terminal (recomendado):**
```powershell
cd C:\Users\Acer\Documents\GitHub\ScheduleClin_Node.js\ScheduleClin_Node
npm run lint
```

**O que mostrar:** lista de erros + linha final, por exemplo:
`✖ XX problems (XX errors, X warnings)`

**Opção B — Arquivo já salvo:**
```
ScheduleClin_Node/docs/eslint-antes.txt
```
Abra com Bloco de Notas e tire print.

---

## Print 4 — Correção automática

**Comando:**
```powershell
npm run lint:fix
```

**O que mostrar:** saída do terminal (correções ou mensagens restantes).

---

## Print 5 — Execução DEPOIS (obrigatório)

**Comando:**
```powershell
npm run lint
```

**O que mostrar:** terminal **vazio** (zero erros) = sucesso.

Salve também em:
```powershell
npm run lint 2>&1 | Tee-Object -FilePath docs\eslint-depois.txt
```

---

## Print 6 — Aplicação funcionando (opcional)

**Onde:** navegador após:
```powershell
npm run dev
```
URL: http://localhost:3000/account/login

---

## Ordem dos comandos (copiar e colar)

```powershell
cd C:\Users\Acer\Documents\GitHub\ScheduleClin_Node.js\ScheduleClin_Node

npm run lint          # PRINT 3 — ANTES
npm run lint:fix      # PRINT 4 — FIX
npm run lint          # PRINT 5 — DEPOIS (vazio)
```

---

## Arquivos de evidência nesta pasta

| Arquivo | Conteúdo |
|---------|----------|
| `eslint-antes.txt` | Saída completa do lint ANTES |
| `eslint-antes.json` | Relatório JSON (contagem automática) |
| `eslint-depois.txt` | Você cria após o passo 5 |
| `EVIDENCIAS_ESLINT.md` | Este guia |

---

## Números para o relatório (antes do fix)

Execute:
```powershell
node -e "const r=require('./docs/eslint-antes.json'); let e=0,w=0; r.forEach(f=>f.messages.forEach(m=>m.severity===2?e++:w++)); console.log('Erros:',e,'Avisos:',w);"
```

Use esses valores na Parte 4 do relatório.

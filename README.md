# 🏥 Clínica de Psicologia — Sistema de Agendamento

[![status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)](#)
[![node](https://img.shields.io/badge/Node.js-18+-green)](https://nodejs.org/)
[![express](https://img.shields.io/badge/Express-4.x-lightgrey)](https://expressjs.com/)
[![SQLServer](https://img.shields.io/badge/SQL%20Server-database-blue)](https://www.microsoft.com/pt-br/sql-server)
[![license](https://img.shields.io/badge/license-academic-lightgrey)](#)

---

> Trabalho apresentado à disciplina **Segurança da Informação**, orientado pelo professor **Edson Vaz Lopes**.

---

## 📌 Domínio do Problema

Desenvolvimento de um software de agendamento para uma clínica de psicologia, com foco em **segurança da informação** aplicada a um sistema web real.

O sistema tem como objetivo gerenciar o agendamento de consultas, oferecendo acesso personalizado e seguro para cada tipo de usuário — garantindo que cada perfil visualize e opere apenas as informações pertinentes ao seu papel na clínica.

---

## 🎯 Objetivo

Desenvolver uma aplicação web utilizando:

- **Node.js + Express** — backend com arquitetura MVC
- **SQL Server** — banco de dados relacional
- **EJS** — templates renderizados no servidor
- **Prisma** — ORM e acesso ao banco

Aplicando boas práticas de **segurança da informação**, controle de acesso por perfil e proteção de dados sensíveis.

---

## 👥 Perfis de Usuário

### 🔑 Gestor (Administrador)
- Acesso total ao sistema
- Gerencia todos os usuários
- Visualiza, altera e cancela qualquer consulta
- Configura parâmetros gerais do sistema

### 🧠 Psicólogo
- Visualiza sua própria agenda de consultas
- Pode cancelar ou solicitar reagendamento de consultas
- Acompanha o histórico de atendimentos dos seus pacientes

### 🙍 Paciente
- Visualiza seus próprios agendamentos confirmados
- Acessa seu histórico de consultas
- Não realiza agendamentos diretamente

---

## 🚀 Funcionalidades

### 🔐 Autenticação e Acesso
- Login com e-mail e senha
- Troca obrigatória de senha no primeiro acesso (senha provisória definida pelo Administrador)
- Controle de acesso baseado em perfil (Admin, Psicólogo, Paciente)
- Cadastro de paciente pela secretária ou diretamente pelo próprio paciente

### 👤 Gerenciamento de Usuários
- Cadastro de pacientes pela secretária com senha padrão provisória
- Edição e inativação de usuários pelo administrador

### 📅 Agendamento
- Criação de consultas pelo administrador, psicólogo.
- Vinculação da consulta ao paciente e ao psicólogo
- Reagendamento e cancelamento de consultas
- Visualização da agenda geral pelo administrador

### 📋 Histórico
- Listagem de consultas por paciente
- Visualização do histórico pelo próprio paciente
- Visualização do histórico pelo psicólogo responsável

### 🖥️ Painel por Perfil
- Paciente visualiza apenas suas próprias consultas
- Psicólogo visualiza apenas sua própria agenda
- Administrador acessa todas as informações do sistema

---

## ✅ Requisitos Funcionais (RF)

### 🔐 1. Autenticação e Acesso

**RF01 –** O sistema deve permitir autenticação via login com e-mail e senha.

**RF02 –** O sistema deve exigir troca de senha obrigatória no primeiro acesso do usuário.

**RF03 –** O sistema deve permitir logout, encerrando a sessão do usuário.

**RF04 –** O sistema deve controlar o acesso às funcionalidades com base no perfil do usuário autenticado.

---

### 👤 2. Gerenciamento de Usuários

**RF05 –** O sistema deve permitir que o administrador cadastre novos pacientes com senha provisória padrão.

**RF06 –** O sistema deve permitir que o paciente realize seu próprio cadastro diretamente no sistema.

**RF07 –** O sistema deve permitir que o administrador edite e inative usuários.

---

### 📅 3. Agendamento

**RF08 –** O sistema deve permitir a criação de consultas pela secretária, psicólogo ou paciente.

**RF09 –** O sistema deve vincular cada consulta a um paciente e a um psicólogo.

**RF10 –** O sistema deve permitir reagendamento e cancelamento de consultas.

**RF11 –** O sistema deve exibir a agenda geral da clínica para o administrador.

---

### 📋 4. Histórico

**RF12 –** O sistema deve listar o histórico de consultas do paciente logado.

**RF13 –** O sistema deve permitir que o psicólogo visualize o histórico de atendimentos dos seus pacientes.

**RF14 –** O sistema deve exibir apenas as consultas vinculadas ao usuário logado, de acordo com seu perfil.

---

## 📑 Requisitos Não Funcionais (RNF)

### 🏗️ 1. Arquitetura e Plataforma

**RNF01 –** O sistema deve ser uma aplicação web acessível via navegador.

**RNF02 –** A arquitetura deve seguir o padrão MVC (Model-View-Controller), separando claramente as responsabilidades de cada camada.

---

### 🔒 2. Segurança

**RNF03 –** As senhas dos usuários devem ser armazenadas de forma criptografada utilizando hash seguro.

**RNF04 –** O sistema deve implementar controle de sessão com expiração automática.

**RNF05 –** O sistema deve garantir que cada usuário acesse apenas as informações autorizadas ao seu perfil.

**RNF06 –** O sistema deve possuir proteção contra ataques comuns (SQL Injection, XSS, CSRF).

**RNF07 –** Rotas e funcionalidades restritas devem ser protegidas e acessíveis apenas aos perfis autorizados.

**RNF08 –** O sistema deve registrar logs de acesso e operações críticas para fins de auditoria.

---

### 🗄️ 3. Banco de Dados

**RNF09 –** O sistema deve utilizar banco de dados relacional SQL Server com integridade referencial garantida por chaves estrangeiras.

**RNF10 –** O esquema do banco deve ser versionado e documentado.

---

### 🔄 4. Manutenibilidade

**RNF11 –** O código deve seguir o padrão MVC com separação clara entre routes, services e views.

**RNF12 –** O projeto deve possuir documentação técnica (este README).

---

## 🛠️ Tecnologias

| Tecnologia | Função | Justificativa |
|---|---|---|
| **Node.js 18+** | Runtime do backend | Ecossistema maduro, assíncrono e amplamente adotado |
| **Express** | Framework HTTP e roteamento | Leve, flexível e compatível com MVC |
| **EJS** | Template engine (frontend) | Renderização dinâmica de HTML no servidor |
| **Prisma** | ORM / acesso ao banco | Tipagem, migrations e integração com SQL Server |
| **SQL Server** | Banco de dados relacional | Persistência com integridade referencial |
| **express-session** | Sessão e autenticação | Controle de sessão com expiração automática (RNF04) |
| **csrf-csrf** | Proteção CSRF | Validação de token em requisições que alteram estado (RNF06) |

---

## 🏗️ Arquitetura (Modelo C4)

### 🔹 Nível 1 – Contexto
O usuário acessa o sistema via navegador. A aplicação processa as requisições via backend Node.js e persiste os dados no **SQL Server**.

```
[Usuário] → [Navegador] → [App Node.js + Express] → [SQL Server]
```

### 🔹 Nível 2 – Contêineres

| Contêiner | Tecnologia | Responsabilidade |
|---|---|---|
| Frontend | EJS, HTML, CSS, Bootstrap | Renderização da interface e interação com o usuário |
| Backend | Node.js, Express | Regras de negócio, autenticação, APIs REST e controle de acesso |
| Database | SQL Server + Prisma | Persistência e integridade dos dados |

### 🔹 Nível 3 – Componentes

**Backend (`ScheduleClin_Node/src/`):**
- `routes/pages/` — Rotas MVC (renderizam views EJS por perfil)
- `routes/api/` — APIs REST (usuários, agenda, consultas)
- `services/` — Regras de negócio
- `middleware/` — Autenticação, CSRF e auditoria
- `views/` — Templates EJS

### 🔹 Nível 4 – Código (Exemplo)

```javascript
router.get('/users', requireRole('Gestor'), (req, res) => {
  res.render('admin/users', { layout: 'partials/admin-layout', ... });
});
```

---

## 📁 Estrutura de Pastas

```
ScheduleClin/                          ← Raiz do repositório
│
├── package.json                       ← Scripts para rodar o Node.js na raiz
├── README.md                          ← Documentação do projeto
│
└── ScheduleClin_Node/                 ← Aplicação Node.js + Express
    ├── package.json
    ├── .env.example                   ← Variáveis de ambiente (copiar para .env)
    ├── prisma/schema.prisma           ← Schema do banco (MariaDB/MySQL)
    ├── src/
    │   ├── server.js                  ← Ponto de entrada
    │   ├── app.js                     ← Configuração Express
    │   ├── routes/pages/              ← Rotas MVC (Admin, Psicólogo, Paciente, Account)
    │   ├── routes/api/                ← APIs REST
    │   ├── services/                  ← Regras de negócio
    │   ├── middleware/                ← Auth, CSRF, auditoria
    │   └── data/seed.js               ← Seed de perfis, papéis e gestor inicial
    ├── views/                         ← Templates EJS
    └── public/                        ← CSS, JS e bibliotecas estáticas
```

---

## 🚀 Como executar (Node.js)

### Pré-requisitos

- Node.js 18+
- **XAMPP** (MariaDB/MySQL) ou MySQL local na porta **3306**
- Banco `scheduleclin` criado no servidor

### Banco com XAMPP (MariaDB)

1. Abra o painel do XAMPP:
   ```
   C:\Users\Acer\Documents\GitHub\xampp\xampp\xampp-control.exe
   ```
2. Clique em **Start** no módulo **MySQL** (MariaDB).
3. Abra **phpMyAdmin**: http://localhost/phpmyadmin
4. Crie um banco chamado **`scheduleclin`** (collation `utf8mb4_unicode_ci`).
5. Configure o `.env` (senha vazia é o padrão do XAMPP):

```env
DATABASE_URL="mysql://root:@localhost:3306/scheduleclin"
```

Se você definiu senha para o `root`:

```env
DATABASE_URL="mysql://root:SUA_SENHA@localhost:3306/scheduleclin"
```

> **Importante:** no seu PC o serviço **MySQL80** também usa a porta 3306. Antes de usar o XAMPP, pare o MySQL80:
> ```powershell
> Stop-Service MySQL80
> ```
> Depois inicie o MySQL pelo XAMPP Control Panel. Para voltar ao MySQL80: `Start-Service MySQL80`

### Configuração

```bash
# Na raiz do repositório
npm install

cd ScheduleClin_Node
copy .env.example .env   # Windows — ajuste DATABASE_URL e SESSION_SECRET
npm run prisma:generate
npm run db:check         # testa conexão com o banco
npm run prisma:push      # cria/atualiza tabelas no banco vazio
```

### Executar

```bash
# Na raiz
npm run dev

# Ou dentro de ScheduleClin_Node
npm run dev
```

Acesse `http://localhost:3000`. Em desenvolvimento, a documentação Swagger fica em `/api-docs`.

**Usuário gestor inicial (seed):** `gestor@scheduleclin.local` / `Gestor@123` (troca de senha obrigatória no primeiro acesso).

---

### 📌 O que cada camada faz?

#### 🟢 Routes + Services (`ScheduleClin_Node/src/`)
Recebem requisições HTTP, aplicam regras de negócio, validam permissões por perfil e retornam views ou JSON.

#### 🟣 Views (`ScheduleClin_Node/views/`)
Templates **EJS** com HTML dinâmico — painéis de Gestor, Psicólogo e Paciente.

#### 🌐 Public (`ScheduleClin_Node/public/`)
Arquivos estáticos (CSS, JS, Bootstrap, jQuery) servidos diretamente pelo Express.

## Banco de Dados

### usuario
id
nome
email
senha
cpf
telefone
dt_nascimento
criado_em
perfil



### perfil
id
funcao "nome"
crp



### agenda
id
dt_hora_min_ag
criado_em
paciente
agendado_por
psicologo

---

## 📌 Status do Projeto

**Em desenvolvimento** — stack Node.js + Express + EJS + Prisma.

---

## 📄 Licença

Projeto acadêmico — uso educacional.

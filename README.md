# 🏥 Clínica de Psicologia — Sistema de Agendamento

[![status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)](#)
[![dotnet](https://img.shields.io/badge/.NET-8.0+-green)](https://dotnet.microsoft.com/)
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

- **.NET 8.0** — backend com arquitetura MVC
- **SQL Server** — banco de dados relacional
- **ASP.NET Core MVC (Razor Views)** — frontend renderizado no servidor

Aplicando boas práticas de **segurança da informação**, controle de acesso por perfil e proteção de dados sensíveis.

---

## 👥 Perfis de Usuário

### 🔑 Gestor (Administrador)
- Acesso total ao sistema
- Gerencia todos os usuários
- Visualiza, altera e cancela qualquer consulta
- Configura parâmetros gerais do sistema

### 🗂️ Secretária
- Cadastra novos pacientes e cria credenciais de acesso
- Realiza agendamento, reagendamento e cancelamento de consultas
- Visualiza a agenda geral da clínica

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
- Troca obrigatória de senha no primeiro acesso (senha provisória definida pela secretária)
- Controle de acesso baseado em perfil (Admin, Secretária, Psicólogo, Paciente)
- Cadastro de paciente pela secretária ou diretamente pelo próprio paciente

### 👤 Gerenciamento de Usuários
- Cadastro de pacientes pela secretária com senha padrão provisória
- Edição e inativação de usuários pelo administrador

### 📅 Agendamento
- Criação de consultas pela secretária, psicólogo ou paciente
- Vinculação da consulta ao paciente e ao psicólogo
- Reagendamento e cancelamento de consultas
- Visualização da agenda geral pela secretária

### 📋 Histórico
- Listagem de consultas por paciente
- Visualização do histórico pelo próprio paciente
- Visualização do histórico pelo psicólogo responsável

### 🖥️ Painel por Perfil
- Paciente visualiza apenas suas próprias consultas
- Psicólogo visualiza apenas sua própria agenda
- Secretária visualiza toda a agenda da clínica
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

**RF05 –** O sistema deve permitir que a secretária cadastre novos pacientes com senha provisória padrão.

**RF06 –** O sistema deve permitir que o paciente realize seu próprio cadastro diretamente no sistema.

**RF07 –** O sistema deve permitir que o administrador edite e inative usuários.

---

### 📅 3. Agendamento

**RF08 –** O sistema deve permitir a criação de consultas pela secretária, psicólogo ou paciente.

**RF09 –** O sistema deve vincular cada consulta a um paciente e a um psicólogo.

**RF10 –** O sistema deve permitir reagendamento e cancelamento de consultas.

**RF11 –** O sistema deve exibir a agenda geral da clínica para a secretária.

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

**RNF11 –** O código deve seguir o padrão MVC com separação clara entre Controllers, Models e Views.

**RNF12 –** O projeto deve possuir documentação técnica (este README).

---

## 🛠️ Tecnologias

| Tecnologia | Função | Justificativa |
|---|---|---|
| **.NET 8.0** | Runtime e framework do backend | Moderno, performático e com amplo suporte a recursos de segurança nativos |
| **ASP.NET Core MVC** | Arquitetura da aplicação | Separação clara de responsabilidades entre Controllers, Models e Views |
| **SQL Server** | Banco de dados relacional | Robusto, confiável e com integração nativa ao ecossistema .NET |
| **Razor Views** | Template engine (frontend) | Renderização dinâmica de HTML no servidor integrada ao ciclo MVC do ASP.NET |
| **ASP.NET Core Identity** | Autenticação e autorização | Gerenciamento seguro de usuários, senhas e perfis de acesso |
| **Data Protection API** | Criptografia de dados sensíveis | Proteção nativa do .NET para dados em repouso e em trânsito |

---

## 🏗️ Arquitetura (Modelo C4)

### 🔹 Nível 1 – Contexto
O usuário acessa o sistema via navegador. A aplicação processa as requisições via backend .NET e persiste os dados no **SQL Server**.

```
[Usuário] → [Navegador] → [App .NET 8 MVC] → [SQL Server]
```

### 🔹 Nível 2 – Contêineres

| Contêiner | Tecnologia | Responsabilidade |
|---|---|---|
| Frontend | Razor Views, HTML, CSS | Renderização da interface e interação com o usuário |
| Backend | ASP.NET Core MVC (.NET 8) | Processamento das regras de negócio, autenticação e controle de acesso |
| Database | SQL Server | Persistência e integridade dos dados |

### 🔹 Nível 3 – Componentes

**Backend:**
- `Controllers` — Recebem as requisições, aplicam regras de negócio e retornam respostas
- `Models` — Representam as entidades do domínio e a comunicação com o banco de dados
- `Views` — Templates Razor renderizados pelo servidor com dados injetados pelo Controller

### 🔹 Nível 4 – Código (Exemplo)

```csharp
[Authorize(Roles = "Secretaria,Admin")]
public IActionResult Index()
{
    var consultas = _consultaService.ObterAgendaGeral();
    return View(consultas);
}
```

---

## 📁 Estrutura de Pastas

```
Projeto/
│
├── Controllers/
│   └── ProdutoController.cs       # Lógica de negócio e tratamento das requisições
│
├── Models/
│   └── Produto.cs                 # Entidades do domínio e acesso a dados
│
├── Views/
│   └── Produto/
│       ├── Index.cshtml           # Listagem de registros
│       └── Details.cshtml        # Detalhes de um registro
│
├── wwwroot/                       # Arquivos estáticos (CSS, JS, imagens)
│
└── Program.cs                     # Ponto de entrada — configuração da aplicação
```

### 📌 O que cada camada faz?

#### 🟢 Controllers (`/Controllers`)
Recebem as requisições HTTP, aplicam as regras de negócio, validam permissões de acesso por perfil e retornam a resposta adequada — renderizando uma View ou redirecionando o usuário.

---

#### 🟡 Models (`/Models`)
Representam as entidades do domínio (Usuário, Consulta, Paciente, Psicólogo etc.) e encapsulam a comunicação com o banco de dados SQL Server.

---

#### 🟣 Views (`/Views`)
Templates **Razor (.cshtml)** que mesclam HTML com dados dinâmicos injetados pelo servidor. São renderizados e enviados prontos ao navegador.

---

#### 🌐 wwwroot (`/wwwroot`)
Arquivos **estáticos** (CSS, JS do cliente, imagens) servidos diretamente pelo servidor sem passar pelos Controllers.

---

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

🚧 **Em desenvolvimento**

---

## 📄 Licença

Projeto acadêmico — uso educacional.

# Ficha Digital — Manuscrito Estudio

[![CI](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/ci.yml/badge.svg)](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/ci.yml)
[![Deploy Azure](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/main_fichadigital.yml/badge.svg)](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/main_fichadigital.yml)

Aplicação full stack criada para substituir fichas de anamnese em papel por um
fluxo digital seguro, rastreável e acessível pelo celular em estúdios de
tatuagem e piercing.

[Acessar a aplicação publicada](https://fichadigital-f0ffagenh8gegvea.eastus-01.azurewebsites.net/profissional/entrar)

> O acesso profissional é protegido por autenticação. O projeto está publicado
> como MVP de demonstração e ainda não deve receber dados pessoais reais.

## Visão geral

O projeto nasceu de um problema real: o estúdio utilizava formulários em papel,
o que dificultava a leitura, a localização de fichas antigas e a preservação do
histórico de cada cliente.

A solução permite que o profissional cadastre o cliente, gere um link
temporário e o envie pelo aplicativo de mensagens de sua preferência. O cliente
abre o link no celular, responde ao histórico de saúde e registra o aceite do
termo. Ao final, o profissional acompanha a confirmação em uma área protegida.

```mermaid
flowchart LR
    A["Profissional autenticado"] --> B["Cadastra o cliente"]
    B --> C["Gera convite válido por 1 hora"]
    C --> D["Cliente recebe o link"]
    D --> E["Responde à ficha pelo celular"]
    E --> F["Registra o aceite"]
    F --> G["Profissional consulta a ficha concluída"]
```

## Demonstração visual

Esta seção está preparada para apresentar as principais etapas do produto:

1. tela de login;
2. painel do profissional;
3. link de convite enviado ao cliente;
4. clientes cadastrados.

<!--
Adicione os arquivos em docs/screenshots e remova este comentário.

| Acesso profissional | Painel do profissional |
| :---: | :---: |
| ![Tela de login](docs/screenshots/login.png) | ![Painel do profissional](docs/screenshots/painel-profissional.png) |

| Convite enviado | Clientes cadastrados |
| :---: | :---: |
| ![Link enviado ao cliente](docs/screenshots/convite-enviado.png) | ![Clientes cadastrados](docs/screenshots/clientes-cadastrados.png) |
-->

## Funcionalidades do MVP

### Área profissional

- autenticação com sessão protegida;
- cadastro de clientes;
- listagem paginada de clientes;
- geração de convite com validade de 1 hora;
- link completo pronto para cópia e compartilhamento;
- acompanhamento do estado das fichas;
- consulta protegida dos dados preenchidos e do resumo do aceite;
- separação entre listagens administrativas e informações sensíveis.

### Experiência do cliente

- abertura da ficha por link temporário;
- validação segura do convite;
- questionário de saúde com perguntas condicionais;
- retomada do fluxo pelo link original;
- apresentação e aceite do termo de consentimento;
- confirmação da conclusão da ficha.

## Destaques técnicos

- **Monólito modular:** mantém a implantação simples sem misturar os domínios
  de clientes, fichas e profissionais.
- **Tokens seguros:** o token original do convite é exibido somente na emissão;
  apenas seu hash é persistido no banco.
- **Segurança em camadas:** cookies `HttpOnly`, proteção antifalsificação,
  limitação de requisições públicas, bloqueio por tentativas de login e
  respostas sensíveis sem cache.
- **Contratos HTTP explícitos:** DTOs de entrada e saída impedem que entidades
  do domínio sejam expostas diretamente.
- **Validação em duas fronteiras:** dados inválidos são rejeitados tanto na API
  quanto pelas regras internas do domínio.
- **Qualidade automatizada:** testes unitários e de integração, lint e build do
  frontend executados pelo GitHub Actions.
- **Entrega contínua:** publicação no Azure App Service por OIDC, sem senha de
  implantação armazenada no workflow.

## Arquitetura

A aplicação utiliza React e TypeScript no frontend, ASP.NET Core no backend e
SQL Server para persistência. O Entity Framework Core mantém o schema do banco
versionado por migrations.

```mermaid
flowchart LR
    A["React + TypeScript"] -->|"HTTPS / JSON"| B["ASP.NET Core API"]
    B --> C["Módulos de negócio"]
    C --> D["Entity Framework Core"]
    D --> E["SQL Server / Azure SQL"]
    B --> F["ASP.NET Core Identity"]
    G["GitHub Actions"] --> H["Azure App Service"]
```

```text
Modules/
├── Clientes/
│   ├── Api/             # Controllers e contratos HTTP
│   ├── Domain/          # Entidades e regras de negócio
│   └── Infrastructure/  # Persistência e mapeamentos
├── Fichas/
└── Profissionais/
```

## Tecnologias

| Camada | Tecnologias |
| --- | --- |
| Backend | C#, .NET 10, ASP.NET Core Web API, Entity Framework Core 10 |
| Autenticação | ASP.NET Core Identity, cookies seguros e antiforgery |
| Frontend | React 19, TypeScript, Vite e CSS responsivo |
| Banco de dados | SQL Server LocalDB e Azure SQL Database |
| Testes | xUnit v3, testes unitários e de integração |
| DevOps | GitHub Actions, Azure App Service e autenticação OIDC |

## Estrutura do repositório

```text
FichaDigital/
├── .github/workflows/                 # CI e publicação no Azure
├── src/
│   ├── backend/FichaDigital.Api/      # API e domínio da aplicação
│   └── frontend/                      # Interface React
├── tests/backend/
│   ├── FichaDigital.UnitTests/
│   └── FichaDigital.IntegrationTests/
├── FichaDigital.sln
└── README.md
```

## Executando localmente

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0);
- [Node.js 24](https://nodejs.org/);
- SQL Server LocalDB ou outra instância do SQL Server;
- npm.

Na raiz do repositório, restaure as dependências e aplique as migrations:

```powershell
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef database update `
  --project src/backend/FichaDigital.Api `
  --startup-project src/backend/FichaDigital.Api
```

Configure uma conta profissional usando o
[Secret Manager do .NET](https://learn.microsoft.com/aspnet/core/security/app-secrets)
nas chaves abaixo, sem versionar a senha:

```text
ProfissionalDesenvolvimento:NomeCompleto
ProfissionalDesenvolvimento:Email
ProfissionalDesenvolvimento:Senha
```

Inicie a API e o frontend em terminais separados:

```powershell
dotnet run --project src/backend/FichaDigital.Api --launch-profile http
```

```powershell
npm --prefix src/frontend install
npm --prefix src/frontend run dev
```

O frontend será disponibilizado normalmente em `http://localhost:5173` e a API
em `http://localhost:5057`.

## Principais endpoints

| Método | Rota | Finalidade |
| --- | --- | --- |
| `POST` | `/api/autenticacao/entrar` | Iniciar a sessão profissional |
| `GET` | `/api/clientes` | Listar clientes com paginação |
| `POST` | `/api/clientes` | Cadastrar um cliente |
| `POST` | `/api/clientes/{clienteId}/fichas/convites` | Gerar uma ficha e seu convite |
| `POST` | `/api/fichas/convites/abrir` | Validar o convite público |
| `POST` | `/api/fichas/questionario-saude` | Registrar o questionário |
| `POST` | `/api/fichas/termo-consentimento/aceitar` | Registrar o aceite e concluir a ficha |
| `GET` | `/api/fichas` | Acompanhar fichas com paginação |

## Próximas evoluções

- histórico de procedimentos preenchido pelo profissional;
- perfis de acesso e autorização por função;
- decisões de negócio do módulo de procedimentos validadas com o estúdio;
- auditoria de acessos quando o sistema entrar em operação real;
- revisão jurídica do termo de consentimento;
- estratégia de backup, retenção e preparação para produção.

## Privacidade e segurança

O sistema foi projetado considerando que informações de saúde são dados
pessoais sensíveis. Mesmo com controles técnicos já implementados, o MVP ainda
precisa de revisão jurídica, política de retenção, auditoria e validação de
produção antes de receber dados reais.

Consulte a [política de segurança](SECURITY.md) para conhecer as orientações do
repositório.

## Autor

Desenvolvido por [Felipe Everardo](https://github.com/Felipe-everardo) como
solução para um problema real e projeto de portfólio full stack.

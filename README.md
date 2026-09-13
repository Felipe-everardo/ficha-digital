# Ficha Digital — Manuscrito Estudio

[![CI](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/ci.yml/badge.svg)](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/ci.yml)
[![Deploy Azure](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/main_fichadigital.yml/badge.svg)](https://github.com/Felipe-everardo/ficha-digital/actions/workflows/main_fichadigital.yml)

Aplicação full stack criada para substituir fichas de anamnese em papel por um
fluxo digital seguro, rastreável e acessível pelo celular em estúdios de
tatuagem e piercing.

[Acessar a aplicação publicada](https://fichadigital-f0ffagenh8gegvea.eastus-01.azurewebsites.net/profissional/entrar)

### Acesso de demonstração

Para conhecer o fluxo da área profissional, utilize a conta de demonstração:

```text
E-mail: feeverardo@gmail.com
Senha: @FePassword123
```

Essa conta e os dados disponíveis no ambiente são exclusivamente fictícios e
destinados à avaliação do projeto.

> O acesso profissional é protegido por autenticação. O projeto está publicado
> como MVP de demonstração e ainda não deve receber dados pessoais reais.

## Visão geral

O projeto nasceu de um problema real: o estúdio utilizava formulários em papel,
o que dificultava a leitura, a localização de fichas antigas e a preservação do
histórico de cada cliente.

A solução permite que o estúdio cadastre apenas um nome de referência,
informe o profissional responsável, selecione tatuagem ou piercing e gere um link
temporário para envio pelo aplicativo de mensagens de sua preferência. O
cliente abre o link no celular, completa os próprios dados e responde ao
histórico de saúde. Na mesma sequência, revisa o termo, confirma que leu e
entendeu, informa o nome e assina. O profissional recebe a ficha autorizada,
confere os dados e a identidade do cliente e registra uma única revisão. Depois
do procedimento, preenche os dados técnicos, o pagamento e sua assinatura.

```mermaid
flowchart LR
    A["Conta do estúdio autenticada"] --> B["Informa um nome de referência"]
    B --> C["Informa o responsável, seleciona o procedimento e gera o convite"]
    C --> D["Cliente recebe o link"]
    D --> E["Cliente completa os dados pessoais"]
    E --> F["Responde à ficha pelo celular"]
    F --> G["Cliente revisa o termo, autoriza e assina"]
    G --> H["Profissional revisa a ficha e a identidade"]
    H --> I["Procedimento é realizado"]
    I --> J["Profissional conclui o registro pós-procedimento"]
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
- uma conta universal do estúdio para manter a operação do MVP simples;
- cadastro inicial do cliente somente por nome de referência;
- busca de clientes por nome, contato e dados da ficha mais recente;
- histórico de fichas e procedimentos por cliente;
- geração de convite com validade de 1 hora, procedimento e nome do profissional
  responsável registrados na ficha;
- link completo pronto para cópia e compartilhamento;
- histórico completo das fichas, mantendo a ficha mais recente em
  destaque na listagem de clientes;
- fichas criadas no dia exibidas automaticamente, da mais recente para a mais
  antiga;
- consulta protegida dos dados preenchidos e do resumo do aceite;
- revisão profissional bloqueada até existir consentimento íntegro;
- confirmação conjunta dos dados da ficha e da identidade do cliente;
- registro posterior específico para tatuagem ou piercing;
- registro de valor, sinal e pagamento por Pix, dinheiro ou cartão;
- preservação dos dados pessoais confirmados em cada ficha, sem reescrever o
  histórico quando o cadastro geral do cliente for atualizado;
- separação entre listagens administrativas e informações sensíveis.

### Experiência do cliente

- abertura da ficha por link temporário;
- validação segura do convite;
- identificação do procedimento e do profissional responsável;
- preenchimento dos próprios dados pessoais e de contato;
- revisão e atualização dos dados anteriores quando o cliente retorna para um
  novo atendimento;
- questionário de saúde com perguntas condicionais;
- retomada do fluxo pelo link original;
- revisão dos dados e do termo em uma sequência contínua;
- um único aceite de leitura e autorização, nome digitado e assinatura
  desenhada;
- autorização separada da conclusão técnica do atendimento.

## Destaques técnicos

- **Monólito modular:** mantém a implantação simples sem misturar os domínios
  de clientes, fichas e profissionais.
- **Tokens seguros:** o token original do convite é exibido somente na emissão;
  apenas seu hash é persistido no banco.
- **Segurança em camadas:** cookies `HttpOnly`, proteção antifalsificação,
  limitação de requisições públicas, bloqueio por tentativas de login e
  respostas sensíveis sem cache.
- **Auditoria sem conteúdo clínico:** registra quem acessou ou alterou o
  recurso, horário, ação e código de correlação sem duplicar dados sensíveis.
- **Operações idempotentes:** reenvios com a mesma chave não duplicam cadastros,
  convites ou registros; respostas temporárias ficam protegidas no banco.
- **Observabilidade:** health checks separados para processo e banco, erros no
  padrão `ProblemDetails` e correlação ponta a ponta.
- **Contratos HTTP explícitos:** DTOs de entrada e saída impedem que entidades
  do domínio sejam expostas diretamente.
- **Histórico imutável:** cada ficha mantém um retrato dos dados pessoais
  confirmados pelo cliente naquele preenchimento.
- **Evidência versionada:** modelo, dados, questionário, termo, aceite e
  assinaturas são preservados com códigos de integridade.
- **Validação em duas fronteiras:** dados inválidos são rejeitados tanto na API
  quanto pelas regras internas do domínio.
- **Qualidade automatizada:** testes unitários, de integração e Playwright no
  navegador, além de lint e build executados pelo GitHub Actions.
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
│   ├── Application/     # Casos de uso e consultas da aplicação
│   ├── Domain/          # Entidades e regras de negócio
│   └── Infrastructure/  # Persistência e mapeamentos
├── Fichas/
└── Profissionais/
```

No frontend, as rotas ficam isoladas em `app`, as integrações HTTP são
separadas por módulo em `services` e o fluxo público da ficha distribui estado,
regras de interação e apresentação entre `hooks`, `pages` e `components`.

## Tecnologias

| Camada | Tecnologias |
| --- | --- |
| Backend | C#, .NET 10, ASP.NET Core Web API, Entity Framework Core 10 |
| Autenticação | ASP.NET Core Identity, cookies seguros e antiforgery |
| Frontend | React 19, TypeScript, Vite e CSS responsivo |
| Banco de dados | SQL Server LocalDB e Azure SQL Database |
| Testes | xUnit v3 e Playwright (unitários, integração e navegador) |
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

### Redefinição administrativa da senha no Azure

Se a conta profissional já existir e a senha precisar ser trocada, configure
temporariamente estas variáveis no App Service:

```text
ProfissionalInicial__Habilitado=true
ProfissionalInicial__RedefinirSenhaSeExistente=true
ProfissionalInicial__NomeCompleto=Nome do profissional
ProfissionalInicial__Email=email-da-conta
ProfissionalInicial__Senha=nova-senha
```

Reinicie a aplicação e confirme o acesso com a nova senha. Em seguida,
desabilite `ProfissionalInicial__Habilitado` e
`ProfissionalInicial__RedefinirSenhaSeExistente`, remova
`ProfissionalInicial__Senha` e reinicie novamente. A redefinição também remove
um eventual bloqueio causado por tentativas de login inválidas.

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

### Migrations e limpeza dos dados de demonstração

No ambiente local, a API aplica automaticamente as migrations pendentes antes
de provisionar a conta profissional. Em produção, esse comportamento fica
desativado por padrão para que uma falha de permissão ou conexão no Azure SQL
não impeça a inicialização do App Service.

Depois de validar as migrations separadamente no Azure SQL, a aplicação pode
executá-las no startup por meio da configuração
`DatabaseInitialization__ApplyMigrationsOnStartup=true` no App Service.

Para apagar somente clientes e fichas fictícias, preservando contas
profissionais e o histórico de migrations, execute o script
[`scripts/reset-demo-data.sql`](scripts/reset-demo-data.sql) no banco correto.
O script usa uma transação, respeita a ordem das chaves estrangeiras e exibe a
contagem final das tabelas afetadas.

Para substituir os registros por uma base histórica de demonstração,
execute [`scripts/seed-demo-data.sql`](scripts/seed-demo-data.sql). A carga é
repetível e cria 12 clientes e 16 fichas completas em diferentes dias, meses e
anos, incluindo clientes com mais de uma ficha. As datas são calculadas a
partir do dia da execução para facilitar o teste dos filtros.

A carga também garante os perfis fictícios Lia e Taty, com especialidade em
tatuagem, e Thais, com especialidade em body piercing. Esses perfis não recebem
senha e não podem entrar no aplicativo; a conta profissional já configurada é
preservada.

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -d FichaDigitalDb `
  -b -i "scripts\seed-demo-data.sql"
```

> Confirme o nome do servidor e do banco antes da execução. Os dois scripts
> excluem permanentemente clientes, fichas, dados pessoais confirmados,
> questionários, aceites e convites existentes no banco selecionado.

Use `GET /health/live` para verificar o processo e `GET /health/ready` para
confirmar também a conexão com o banco. O endpoint legado
`GET /api/status/database` continua disponível.

Para executar o teste automatizado do fluxo no Chromium:

```powershell
npm --prefix src/frontend run test:e2e
```

Na primeira execução, instale o navegador com
`npm --prefix src/frontend exec -- playwright install chromium`. O teste sobe API e
frontend isolados com SQLite e dados descartáveis; não utiliza o banco local de
desenvolvimento.

## Principais endpoints

| Método | Rota | Finalidade |
| --- | --- | --- |
| `POST` | `/api/autenticacao/entrar` | Iniciar a sessão profissional |
| `GET` | `/api/status/database` | Verificar a conexão da aplicação com o banco |
| `GET` | `/health/live` | Verificar se o processo está ativo |
| `GET` | `/health/ready` | Verificar se aplicação e banco estão prontos |
| `GET` | `/api/clientes` | Listar e filtrar clientes com paginação |
| `GET` | `/api/clientes/{clienteId}` | Consultar dados e histórico do cliente |
| `POST` | `/api/clientes` | Cadastrar o nome de referência do cliente |
| `POST` | `/api/clientes/{clienteId}/fichas/convites` | Gerar ficha e convite com o procedimento informado |
| `POST` | `/api/fichas/convites/abrir` | Validar o convite público |
| `POST` | `/api/fichas/dados-pessoais` | Registrar os dados informados pelo cliente |
| `POST` | `/api/fichas/questionario-saude` | Registrar o questionário |
| `POST` | `/api/fichas/termo-consentimento/aceitar` | Assinar e autorizar o procedimento |
| `POST` | `/api/fichas/{fichaId}/operacoes/revisar` | Confirmar a revisão profissional da ficha e da identidade |
| `POST` | `/api/fichas/{fichaId}/operacoes/concluir` | Salvar o registro técnico posterior |
| `GET` | `/api/fichas` | Acompanhar e filtrar fichas com paginação |
| `GET` | `/api/auditoria` | Consultar a trilha operacional da conta do estúdio |

## Próximas evoluções

- troca obrigatória da senha inicial e recuperação de acesso;
- perfis individuais e permissões somente se a operação real do estúdio exigir;
- validação jurídica do mecanismo digital e das adaptações do termo;
- substituição do CNPJ fictício pelo dado oficial do estúdio;
- exportação em PDF e Excel, backup e política de retenção;
- atualização da carga de demonstração para exemplificar todos os novos
  estados do atendimento.

## Privacidade e segurança

O sistema foi projetado considerando que informações de saúde são dados
pessoais sensíveis. Mesmo com controle de acesso, auditoria e proteções
técnicas, o MVP ainda precisa de política de retenção e validação completa do
ambiente de produção antes de receber dados reais.

Consulte a [política de segurança](SECURITY.md) para conhecer as orientações do
repositório.

O escopo, as regras e as decisões pendentes desta evolução estão registrados em
[`docs/planejamento-fichas.md`](docs/planejamento-fichas.md).
As decisões arquiteturais ficam em [`docs/decisoes`](docs/decisoes) e os
cuidados de publicação em
[`docs/operacao-producao.md`](docs/operacao-producao.md).

## Autor

Desenvolvido por [Felipe Everardo](https://github.com/Felipe-everardo) como
solução para um problema real e projeto de portfólio full stack.

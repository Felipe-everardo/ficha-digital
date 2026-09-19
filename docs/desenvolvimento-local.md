# Guia de desenvolvimento local

Este documento reúne a preparação do ambiente, os comandos de teste, os dados
de demonstração e as principais rotas da API. Para uma apresentação resumida
do produto, consulte o [README do projeto](../README.md).

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0);
- [Node.js 24](https://nodejs.org/);
- SQL Server LocalDB ou outra instância do SQL Server;
- npm.

## Preparação do ambiente

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

No ambiente local, a API aplica automaticamente as migrations pendentes antes
de provisionar a conta profissional.

## Dados de demonstração

Para apagar somente clientes e fichas fictícias, preservando contas
profissionais e o histórico de migrations, execute
[`scripts/reset-demo-data.sql`](../scripts/reset-demo-data.sql) no banco correto.

Para substituir os registros por uma base histórica de demonstração, execute
[`scripts/seed-demo-data.sql`](../scripts/seed-demo-data.sql). A carga é
repetível e cria clientes e fichas completas em diferentes períodos, incluindo
clientes com mais de uma ficha e profissionais fictícios sem acesso ao sistema.

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -d FichaDigitalDb `
  -b -i "scripts\seed-demo-data.sql"
```

> Confirme o servidor e o banco antes da execução. Os scripts removem
> permanentemente os dados de clientes e fichas do banco selecionado.

## Verificações e testes

Use `GET /health/live` para verificar o processo e `GET /health/ready` para
confirmar também a conexão com o banco.

```powershell
dotnet test FichaDigital.sln
npm --prefix src/frontend run lint
npm --prefix src/frontend run build
npm --prefix src/frontend run test:e2e
```

Na primeira execução dos testes de navegador, instale o Chromium:

```powershell
npm --prefix src/frontend exec -- playwright install chromium
```

O teste de navegador inicia API e frontend isolados com SQLite e dados
descartáveis; ele não utiliza o banco local de desenvolvimento.

### Testes opcionais com SQL Server real

A suíte de integração também valida migrations, consultas e concorrência em um
SQL Server descartável criado com Testcontainers. Ela é executada
automaticamente pelo CI e exige Docker para ser executada localmente.

No PowerShell, com o Docker em execução:

```powershell
$env:RUN_SQLSERVER_TESTS = "true"
dotnet test FichaDigital.sln --configuration Release
Remove-Item Env:RUN_SQLSERVER_TESTS
```

Sem essa variável, os testes que dependem de Docker são ignorados e os testes
rápidos com SQLite continuam sendo executados normalmente.

## Principais endpoints

| Método | Rota | Finalidade |
| --- | --- | --- |
| `POST` | `/api/autenticacao/entrar` | Iniciar a sessão profissional |
| `GET` | `/health/live` | Verificar se o processo está ativo |
| `GET` | `/health/ready` | Verificar se aplicação e banco estão prontos |
| `GET` | `/api/clientes` | Listar e filtrar clientes |
| `GET` | `/api/clientes/{clienteId}` | Consultar cliente e histórico |
| `POST` | `/api/clientes` | Cadastrar uma referência de cliente |
| `POST` | `/api/clientes/{clienteId}/fichas/convites` | Gerar ficha e convite |
| `POST` | `/api/fichas/convites/abrir` | Validar o convite público |
| `POST` | `/api/fichas/dados-pessoais` | Registrar os dados do cliente |
| `POST` | `/api/fichas/questionario-saude` | Registrar o questionário |
| `POST` | `/api/fichas/termo-consentimento/aceitar` | Registrar consentimento e assinatura |
| `POST` | `/api/fichas/{fichaId}/operacoes/revisar` | Confirmar a revisão profissional |
| `POST` | `/api/fichas/{fichaId}/operacoes/concluir` | Salvar o registro técnico posterior |
| `GET` | `/api/fichas` | Acompanhar e filtrar fichas |
| `GET` | `/api/auditoria` | Consultar a trilha operacional |

# Publicação manual do piloto do estúdio

Preparação em 13/10/2026. O código e o workflow estão preparados. O grupo
`rg-fichadigital-estudio`, o plano F1 `asp-fichadigital-estudio` e o App Service
Windows/.NET 10 `fichadigital-estudio` foram criados no Azure, sem Application
Insights. Também foram criados o servidor `sql-fichadigital-estudio` e o banco
gratuito `FichaDigitalEstudioDb` em West US 2. A identidade da aplicação está
ativa e a identidade de deploy tem federação GitHub e permissão de publicação
no App Service do piloto. A estrutura SQL e o acesso limitado da aplicação foram
concluídos. A publicação e a criação da conta exclusiva foram concluídas. Este guia não
declara o sistema pronto para receber dados reais.

Endereço provisionado: `https://fichadigital-estudio-aacpdwfkbpa9f3gx.eastus-01.azurewebsites.net`.
A versão `71ee391` foi implantada nesse endereço em 13/10/2026. A segunda
tentativa da etapa de implantação concluiu com sucesso, e `/health/ready`
respondeu HTTP 200 com `Healthy`. Esses são resultados daquela preparação;
esta recuperação de arquivos em 15/10/2026 não revalidou os recursos remotos.

O inventário completo de bancos do portal mostrou apenas `FichaDigitalDb`,
da demo, na oferta gratuita, em West US 2. A assinatura rejeitou um servidor
SQL novo em East US; o formulário aceitou West US 2 para o servidor exclusivo
`sql-fichadigital-estudio`. A revisão final está preparada com o banco
`FichaDigitalEstudioDb`, autenticação somente Microsoft Entra, administrador
`feeverardo@outlook.com`, oferta gratuita e cobrança por excedente desabilitada.
A confirmação dos termos e do acesso administrativo foi recebida e a
implantação foi concluída no portal. O assistente criou a regra padrão
`AllowAllWindowsAzureIps`; restringir essa regra aos endereços necessários do
piloto antes de liberar dados reais.

As variáveis `ASPNETCORE_ENVIRONMENT=Piloto`, nome e pasta exclusivos de
Data Protection, migrations automáticas desabilitadas e `AllowedHosts` foram
salvas no App Service. A conexão SQL foi salva com autenticação Active Directory
Managed Identity, criptografia obrigatória e validação do certificado. O
provisionamento da conta exclusiva foi concluído. `ProfissionalInicial__Habilitado` foi salvo como `false`, e `ProfissionalInicial__Senha` foi removido da configuração.

## Registro da preparação e verificações pendentes

- Após o bloqueio inicial da revisão automática, o usuário autorizou a identidade
  e o acesso limitado ao banco do piloto. A tela do Azure confirma a identidade
  gerenciada ativa, com object ID `3231994e-1fa9-43f9-b955-73dbf21f596d`.
  A concessão SQL de `db_datareader` e `db_datawriter` em `FichaDigitalEstudioDb`
  foi executada e conferida no portal; nenhuma permissão foi concedida na demo.
- O usuário autorizou liberar temporariamente o IP administrativo `201.18.186.40`.
  A regra `QueryEditorClientIPAddress_1791916583590` foi usada para preparação
  e removida ao terminar; a remoção foi salva e conferida no portal.
- O SQL idempotente das migrations foi gerado em
  `docs/particular/migrations-piloto.sql` (pasta ignorada pelo Git).
  O script de acesso está em `scripts/provisionar-acesso-estudio.sql` e valida
  o nome do banco antes de criar o usuário. Ambos foram executados no banco do piloto.
  Conferência anterior à criação da conta: 24 migrations, 19 tabelas, zero usuários e zero fichas. A conta inicial foi criada depois dessa conferência.
  A instalação limpa e a reaplicação do SQL foram validadas em LocalDB isolado.
  Comandos de dados das migrations usam SQL dinâmico para evitar compilação
  antecipada de colunas recém-criadas ou já removidas em scripts idempotentes.
  Para executar via sqlcmd, usar `-I -b` (QUOTED_IDENTIFIER e parada em erro).
  O CI executa também um teste de instalação e reaplicação idempotente em SQL
  Server descartável, além dos testes de compatibilidade já existentes.
- O environment `estudio` foi conferido no GitHub: apenas branch `main`, duas
  variáveis com os valores corretos do App Service e três environment secrets
  (`AZURE_ESTUDIO_CLIENT_ID`, `AZURE_ESTUDIO_TENANT_ID` e
  `AZURE_ESTUDIO_SUBSCRIPTION_ID`) cadastrados. A identidade de deploy e sua
  federação foram verificadas/configuradas no Azure. O workflow foi enviado
  na proposta [PR #1](https://github.com/Felipe-everardo/ficha-digital/pull/1).
  A proposta foi integrada à main, versão 71ee391. A publicação manual concluiu
  com sucesso na tentativa 2 da etapa de deploy:
  [execução #1](https://github.com/Felipe-everardo/ficha-digital/actions/runs/38450642164).
- O pacote Release foi preparado em uma pasta temporária local, com o perfil
  Piloto e o frontend do estúdio. O workflow montou e enviou o pacote ao App Service.
- Verificação local: 169 testes .NET aprovados, 4 de SQL Server ignorados por
  exigir Docker; 5 E2E aprovados; lint, builds demo/estúdio e publicação Release
  aprovados. A dependência transitiva `source-map-js` foi atualizada para 1.2.2;
  `npm audit` passou sem alertas após a atualização.

As configurações e o banco da demo foram preservados. A integração à main
disparou seu fluxo automático de publicação. Na preparação, a prontidão da demo
foi conferida e retornou Healthy.

## Diagnóstico e conclusão da primeira publicação

Registro reconstruído do histórico de 13/10/2026:

- O primeiro deploy enviou o pacote, mas falhou na verificação de prontidão.
- O log do App Service indicou tempo limite na fase pós-login do SQL. A política
  de conexão do servidor exclusivo foi alterada de Padrão para Proxy e salva,
  mantendo autenticação Entra e criptografia.
- Depois dessa alteração, a aplicação conseguiu consultar `AspNetUsers`, mas a
  criação da conta falhou porque a senha tinha menos de 12 caracteres.
- O usuário corrigiu e salvou a senha no portal; o log passou a registrar
  inicialização bem-sucedida, e a tela de acesso profissional abriu.
- O provisionamento inicial foi desabilitado, e a configuração temporária da
  senha foi removida. A senha não foi lida nem incluída nos arquivos recuperados.
- A etapa de implantação foi repetida com o mesmo pacote. O GitHub registrou
  sucesso, incluindo prontidão e verificação de convite inexistente.

O login com a senha pessoal e o fluxo completo de atendimento devem ser
conferidos pelo operador. Proteção e recuperação das chaves, restauração de
backup e critérios de liberação de dados reais continuam pendentes.

Referência: [diagnóstico de conexões SQL](https://learn.microsoft.com/en-us/azure/sql-database/sql-database-troubleshoot-common-connection-issues).

## Separação

Uma base de código, duas aplicações e dois bancos. O deploy existente
`main_fichadigital.yml` continua responsável pela demo. O novo
`publicar_estudio.yml` só executa por `workflow_dispatch`, na `main`, passando
pelo mesmo CI. Cada execução publica o commit selecionado, identificado pelo
SHA; novos pushes não disparam o piloto. Os grupos de concorrência são distintos.

O build usa `FICHA_AMBIENTE=demo` por padrão e mostra aviso de dados fictícios.
O workflow do piloto define `FICHA_AMBIENTE=estudio`, removendo esse aviso do
pacote do estúdio. A API é acessada no mesmo domínio da aplicação; não configurar
o frontend do piloto para consultar a API da demo.

Não copiar o banco, contas, chaves ou credenciais da demo. Não executar
`scripts/seed-demo-data.sql` nem `scripts/reset-demo-data.sql` no piloto.

## Recursos candidatos sem cobrança adicional

- Grupo: `rg-fichadigital-estudio`.
- App Service: `fichadigital-estudio`, Windows, .NET 10, plano F1 separado.
- Plano: `asp-fichadigital-estudio`.
- Servidor SQL novo: `sql-fichadigital-estudio`, West US 2.
- Banco vazio: `FichaDigitalEstudioDb`, oferta gratuita com pausa ao esgotar a cota.
- Não habilitar Application Insights ou outros recursos pagos neste provisionamento.

Confirmar o resumo de custo e disponibilidade no portal antes de criar. O F1
tem cotas; oferta gratuita não é promessa de disponibilidade contínua. A
restauração nativa do SQL gratuito não tem como destino outro banco da oferta
gratuita. As alternativas e suas limitações estão no
[estudo do orçamento zero](ambiente-estudio-gratuito.md).

## Configuração do App Service

Usar variáveis de ambiente; não salvar segredos no Git ou no workflow:

| Configuração | Valor |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Piloto` |
| `ConnectionStrings__DefaultConnection` | Conexão exclusiva, `Database=FichaDigitalEstudioDb;Encrypt=True;TrustServerCertificate=False` |
| `DatabaseInitialization__ApplyMigrationsOnStartup` | `false` |
| `DataProtection__ApplicationName` | `FichaDigital.Estudio.Piloto` |
| `DataProtection__KeysPath` | Diretório persistente absoluto fora de `wwwroot`, por exemplo `D:\home\data\ficha-estudio-keys` no App Service Windows |
| `AllowedHosts` | Host efetivo do App Service, sem esquema ou caminho |
| `Estudio__Cnpj` | Identificação validada pelo estúdio |

O perfil `Piloto` apaga a conexão padrão no arquivo de configuração e falha
antes da inicialização se faltar configuração, se apontar para o banco/servidor
conhecido da demo, se desabilitar validação do certificado ou se tentar aplicar
migrations ao iniciar. Isso é proteção contra erros conhecidos; não substitui
a verificação das permissões e do destino real no Azure.

Habilitar HTTPS obrigatório. A identidade da aplicação deve ter acesso apenas
ao banco do estúdio e sem DDL. A identidade de publicação não precisa acessar
os dados SQL. Migrations usam identidade administrativa separada, seguindo o
[runbook](operacao-producao.md#migrations-em-produção).

A conta inicial usa `ProfissionalInicial__*`, conforme o runbook. Provisionar
uma conta privada e remover a senha e desativar o provisionamento após validar
o primeiro login. A conta pública da demo não deve existir no novo banco.
O MVP ainda usa conta compartilhada; contas individuais continuam pendentes.

O nome configurável de Data Protection mantém `FichaDigital` como padrão para
a demo, preservando a compatibilidade de suas sessões. No piloto, usar pasta e
nome próprios. Persistir chaves não configura sua criptografia em repouso;
proteção e recuperação das chaves ainda precisam ser validadas antes dos dados reais.

## GitHub Environment `estudio`

Criar o environment `estudio`, limitar publicação à `main` e configurar revisão
manual quando disponível no plano do repositório. Cadastrar exclusivamente nele:

| Tipo | Nome |
| --- | --- |
| Variable | `AZURE_ESTUDIO_APP_NAME` |
| Variable | `AZURE_ESTUDIO_BASE_URL` (URL HTTPS real do aplicativo, sem caminho) |
| Secret | `AZURE_ESTUDIO_CLIENT_ID` |
| Secret | `AZURE_ESTUDIO_TENANT_ID` |
| Secret | `AZURE_ESTUDIO_SUBSCRIPTION_ID` |

Configurar a identidade OIDC de deploy exclusiva, federada ao subject
`repo:Felipe-everardo@183170494/ficha-digital@1311713001:environment:estudio`, com permissão de
publicação limitada ao App Service do piloto. Não copiar a identidade da demo
nem atribuir acesso ao grupo da demo ou à assinatura inteira.

O registro `fichadigital-estudio-deploy` já existe, com client ID
`9d70981c-4654-4a34-a15b-d978b62faede`, tenant ID
`99e48e2d-7c48-4a39-81d5-b8224dd1278a` e service principal object ID
`ceca0ccb-cb5d-4b78-9505-2098a9ec54b0`. A credencial federada `github-estudio`
foi criada e conferida no portal com esse subject, emissor
`https://token.actions.githubusercontent.com` e audience
`api://AzureADTokenExchange`. Não foi criado client secret.

A função `Website Contributor` (no portal em português, `Contribuidor do Site`)
foi atribuída a esse service principal e conferida no IAM do App Service
`fichadigital-estudio`, com escopo `Este recurso`. Não foram atribuídas funções
adicionais no grupo, assinatura ou banco para essa identidade de deploy.

Valores cadastrados como environment secrets no GitHub e conferidos pela lista
de secrets no environment `estudio`:

| Secret | Valor |
| --- | --- |
| `AZURE_ESTUDIO_CLIENT_ID` | `9d70981c-4654-4a34-a15b-d978b62faede` |
| `AZURE_ESTUDIO_TENANT_ID` | `99e48e2d-7c48-4a39-81d5-b8224dd1278a` |
| `AZURE_ESTUDIO_SUBSCRIPTION_ID` | `ae2f5fb5-b537-44c3-84f2-bb4465d012f4` |

Esses valores são identificadores, não senhas ou tokens. O acesso de publicação
depende da federação e das permissões atribuídas, e não apenas desses IDs.

O repositório foi criado em 25/07/2026 e usa o formato padrão com IDs imutáveis
aplicável aos novos repositórios desde 15/07/2026. O formato anterior apenas
com nomes não corresponde a esse padrão. Referência:
[claims imutáveis do GitHub](https://docs.github.com/en/actions/reference/security/oidc).

O workflow rejeita o App Service conhecido da demo e exige uma URL HTTPS
compatível com o nome configurado. Se as variáveis faltarem, falha antes de
autenticar no Azure. Confirmar também a autorização real da identidade.

## Primeira publicação e liberação

1. Criar recursos exclusivos e conferir configurações, identidade e custo.
2. Aplicar migrations no banco vazio com identidade administrativa separada.
3. Disponibilizar este workflow na `main`; a atualização da demo segue o fluxo existente.
4. Em Actions, executar **Publicar piloto do estúdio (manual)** na `main`.
5. Conferir SHA, CI, `/health/ready` e a verificação de convite inexistente.
6. Com dados fictícios, testar login, convite, celular, assinatura, revisão e histórico.
7. Reiniciar e republicar, verificando sessões e chaves. Comprovar que as
   credenciais da demo falham no piloto e que registros não aparecem entre ambientes.
8. Ensaiar recuperação, revisar termos e responsabilidades e registrar a
   liberação do piloto antes do primeiro atendimento real.

Falha no health check marca a publicação como malsucedida; não há rollback
automático. O workflow só aceita a referência `main`, portanto um rollback
de código requer um revert revisado na `main` e validação de compatibilidade
com o esquema atual; não reverter o banco por tentativa.

Referências: [Azure SQL gratuito](https://learn.microsoft.com/azure/azure-sql/database/free-offer)
e [planos do App Service](https://learn.microsoft.com/azure/app-service/overview-hosting-plans).

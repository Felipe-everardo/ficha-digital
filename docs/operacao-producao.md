# Runbook de produção

## Antes de publicar

- Usar uma string de conexão fornecida por secret da hospedagem; nunca salvar
  senha em `appsettings.json`, `.env` versionado ou log.
- Persistir e proteger as chaves do ASP.NET Data Protection. Sem isso, cookies
  e respostas idempotentes podem deixar de ser legíveis após reinício ou troca
  de instância. Configure `DataProtection__KeysPath` com um diretório persistente
  fora da pasta de publicação e valide o login depois de reiniciar a aplicação.
- Criar a conta inicial do estúdio por configuração secreta, validar o
  primeiro acesso e depois desativar `ProfissionalInicial:Habilitado` e remover
  a senha do ambiente.
- Habilitar HTTPS obrigatório, criptografia do banco e backups automáticos
  criptografados. Fazer um teste real de restauração antes de atender clientes.
- Definir com a proprietária e a assessoria jurídica o prazo de retenção das
  fichas, auditoria e backups. Não automatizar exclusão antes dessa decisão.

## Migrations em produção

Em produção, a aplicação não aplica migrations automaticamente por padrão. A
migration deve ser validada separadamente no Azure SQL antes da publicação.

Manter `DatabaseInitialization__ApplyMigrationsOnStartup=false` no App Service.
A identidade gerenciada da aplicação tem acesso de leitura e escrita de dados;
isso não inclui `ALTER TABLE`. Habilitar migrations na inicialização com essa
identidade impede a aplicação de iniciar quando há alteração de esquema pendente.

Aplicar migrations com uma sessão administrativa separada:

1. Consultar a última migration em `dbo.__EFMigrationsHistory`.
2. Executar `dotnet tool restore` e gerar o SQL entre essa versão e a versão
   desejada: `dotnet ef migrations script MIGRATION_ATUAL MIGRATION_DESTINO --project src/backend/FichaDigital.Api --output migration.sql`.
3. Revisar e testar o SQL em banco descartável. Confirmar banco de destino e
   disponibilidade de backup antes de aplicá-lo no Editor de Consultas do Azure.
4. Aplicar a mudança de esquema e o registro em `__EFMigrationsHistory` na mesma
   transação, com reversão em caso de erro. Não registrar uma migration sem
   executar suas alterações.
5. Publicar e validar `/health/ready`, a busca de clientes, o histórico de um
   cliente e os detalhes de uma ficha. O health check retorna 503 se houver
   migrations pendentes no SQL Server ou falha na consulta da versão da ficha.

Não conceder `db_owner` ou permissão de DDL à aplicação para contornar esse fluxo.

## Compartilhamento de convites

O QR Code é gerado no navegador do profissional e contém o mesmo link do botão
de cópia, sem serviço externo. O convite continua válido por uma hora desde a
emissão, para preenchimento e consentimento; a duração do procedimento não está
limitada por esse prazo.

Novos links usam `/fichas/preencher#TOKEN`. O fragmento não acompanha requisições
HTTP de navegação, evitando sua inclusão nos logs de caminho do servidor. O
frontend remove o fragmento do endereço e mantém o token na sessão da aba para
permitir recarregar a página. As APIs recebem o token no corpo do POST; não
habilitar captura de corpos de requisições/respostas no servidor, proxy ou APM.
Links antigos com token no caminho continuam aceitos, mas sua primeira navegação
ainda pode aparecer em logs. Esta mudança não remove registros antigos.

## Redefinição administrativa da senha

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

## Monitoramento mínimo

- Configurar o monitor externo para consultar `GET /health/live` a cada minuto
  e alertar após três falhas consecutivas.
- Configurar a plataforma para usar `GET /health/ready` como verificação de
  prontidão. Uma falha indica indisponibilidade do banco e deve impedir que a
  instância receba tráfego.
- Criar alerta para respostas HTTP 5xx e para falhas repetidas de login ou
  bloqueios de conta. Não incluir corpo de requisição nos alertas.
- Centralizar logs estruturados com acesso limitado. O código de correlação da
  resposta deve acompanhar o chamado de suporte.
- Alertar falhas do serviço de limpeza de idempotência e crescimento inesperado
  das tabelas `RegistrosAuditoria` e `RequisicoesIdempotentes`.

## Resposta a incidente

1. Registrar horário, ambiente, rota e `X-Correlation-ID`; não copiar dados
   clínicos para ferramentas de suporte.
2. Verificar `/health/live` e `/health/ready`.
3. Se houver suspeita de acesso indevido, revogar a conta, preservar logs e a
   auditoria e comunicar a proprietária.
4. Restaurar backup somente depois de validar o ponto de restauração em um
   ambiente isolado.
5. Documentar causa, impacto e ação preventiva depois da recuperação.

## Verificação de cada release

Executar `dotnet test FichaDigital.sln`, `npm run lint`, `npm run build` e
`npm run test:e2e`. Após publicar, validar os dois health checks, login do
estúdio, criação de um cliente de teste e remoção posterior desses dados
conforme a política definida para o estúdio.

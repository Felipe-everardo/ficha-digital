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

Depois dessa validação, o App Service pode aplicá-la durante a inicialização
com a configuração `DatabaseInitialization__ApplyMigrationsOnStartup=true`.
Uma falha de permissão ou conexão nesse processo impede a inicialização, por
isso a configuração deve permanecer desabilitada quando não for necessária.

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

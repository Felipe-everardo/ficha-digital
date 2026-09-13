# Runbook de produção

## Antes de publicar

- Usar uma string de conexão fornecida por secret da hospedagem; nunca salvar
  senha em `appsettings.json`, `.env` versionado ou log.
- Persistir e proteger as chaves do ASP.NET Data Protection. Sem isso, cookies
  e respostas idempotentes podem deixar de ser legíveis após reinício ou troca
  de instância.
- Criar a conta inicial do estúdio por configuração secreta, validar o
  primeiro acesso e depois desativar `ProfissionalInicial:Habilitado` e remover
  a senha do ambiente.
- Habilitar HTTPS obrigatório, criptografia do banco e backups automáticos
  criptografados. Fazer um teste real de restauração antes de atender clientes.
- Definir com a proprietária e a assessoria jurídica o prazo de retenção das
  fichas, auditoria e backups. Não automatizar exclusão antes dessa decisão.

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

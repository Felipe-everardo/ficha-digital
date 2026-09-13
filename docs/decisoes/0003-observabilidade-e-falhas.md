# ADR 0003 — Saúde da aplicação e correlação de falhas

Status: aceito em 11/09/2026.

## Decisão

- `/health/live` confirma que o processo está ativo sem depender do banco.
- `/health/ready` confirma que a aplicação consegue acessar o banco.
- Toda resposta recebe `X-Correlation-ID`; o mesmo valor aparece no
  `ProblemDetails` de falhas não tratadas e deve ser usado para localizar logs.
- Respostas da API usam `Cache-Control: no-store` e cabeçalhos defensivos.
- Logs e auditoria não devem registrar corpo de ficha, token, CPF, assinatura,
  senha ou string de conexão.
- O pipeline executa um teste Playwright do login até a geração de convite.

## Consequências

O código oferece sinais padronizados, mas alertas e retenção de logs dependem do
provedor de hospedagem. A configuração externa obrigatória está no runbook de
produção.

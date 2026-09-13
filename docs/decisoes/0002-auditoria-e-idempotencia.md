# ADR 0002 — Auditoria mínima e operações idempotentes

Status: aceito em 11/09/2026.

## Contexto

Fichas contêm dados pessoais e de saúde. É necessário saber quem consultou ou
alterou recursos e também evitar duplicidade quando uma requisição é reenviada
por instabilidade de rede ou repetição do usuário.

## Decisão

A auditoria persiste identificador do profissional, ação, tipo e identificador
do recurso, horário UTC e código de correlação. Ela nunca copia CPF, respostas
clínicas, assinaturas, tokens ou corpos de requisição. Apenas a proprietária
pode consultar a trilha pela API `/api/auditoria`.

Operações mutáveis podem enviar um UUID no cabeçalho `Idempotency-Key`. A
primeira chamada reserva a chave; chamadas simultâneas recebem conflito sem
executar novamente; chamadas concluídas retornam a resposta original. Um hash
impede reutilizar a mesma chave com outro conteúdo.

O corpo da resposta idempotente é protegido pelo ASP.NET Data Protection e
expira em 24 horas. Um serviço remove registros expirados periodicamente. As
chaves de Data Protection precisam ser persistidas fora da instância em
produção.

## Consequências

O frontend gera uma chave por operação. Erros não são armazenados, permitindo
nova tentativa. A trilha de auditoria é evidência operacional; ela não substitui
backup, controle de acesso da hospedagem nem avaliação jurídica.

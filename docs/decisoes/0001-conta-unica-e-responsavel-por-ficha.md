# ADR 0001 — Conta única e responsável registrado na ficha

Status: aceito em 12/09/2026.

## Contexto

O MVP precisa substituir as fichas de papel e preservar quem foi responsável
por cada atendimento. Criar contas, perfis e permissões para toda a equipe
aumentou a complexidade antes de existir uma necessidade operacional validada.

## Decisão

- O estúdio usa uma única conta autenticada no MVP.
- Ao gerar o convite, o operador informa o nome completo do profissional
  responsável e seleciona tatuagem ou piercing.
- Nome e procedimento são copiados para a ficha e apresentados ao cliente.
- O nome não é digitado novamente na conclusão: a assinatura profissional fica
  vinculada ao responsável que já foi registrado na ficha.
- Se o responsável ou o procedimento estiver errado antes do atendimento, o
  convite deve ser descartado e recriado.
- A conta autenticada continua registrada na auditoria como autora das ações do
  sistema.

## Consequências

O fluxo fica menor e adequado ao MVP. O nome informado identifica o responsável
no documento, mas não substitui autenticação individual nem prova isoladamente
quem estava usando a conta compartilhada. Perfis individuais poderão ser
adicionados no futuro caso a rotina real do estúdio justifique esse custo.

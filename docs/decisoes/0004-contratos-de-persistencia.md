# ADR 0004 — Contratos de persistência na aplicação

Status: aceita.

## Contexto

Os casos de uso de fichas acessavam diretamente o `FichaDigitalDbContext`.
Isso misturava a sequência de decisões do atendimento com consultas do EF Core
e exigia um banco mesmo para testar regras isoladas de aplicação.

## Decisão

A camada de aplicação define interfaces específicas para os casos de uso.
As implementações ficam na infraestrutura e recebem o contexto por injeção
de dependência. Não há repositório genérico nem exposição de `IQueryable`.

- Operações de escrita usam contratos de abertura de convite, dados pessoais,
  questionário, consentimento, revisão e conclusão do procedimento.
- Listagem e detalhes de clientes e fichas usam contratos de consulta que
  retornam os DTOs existentes. Filtros, projeções, ordenação e particularidades
  dos provedores permanecem nas implementações de infraestrutura.
- O cálculo de hash também possui contrato na aplicação; a implementação
  criptográfica continua na infraestrutura.

Os repositórios e o contexto têm escopo de requisição. Entidades modificáveis
obtidas por um repositório são acompanhadas pelo mesmo contexto até sua
gravação. Os contratos de escrita pressupõem esse mesmo escopo; não são uma
API para anexar entidades arbitrárias vindas de outras sessões.

Cada operação confirma as alterações relacionadas em uma única chamada a
`SaveChangesAsync`. Nas etapas públicas, a auditoria é adicionada ao mesmo
contexto antes dessa chamada. A transação do EF Core e a propriedade de
concorrência da ficha permanecem responsáveis pela consistência da gravação.

## Verificação

- Teste de arquitetura inspeciona dependências de tipos e métodos, incluindo
  código gerado para `async`, e impede referências da aplicação ao EF Core,
  à infraestrutura, aos contratos HTTP dos módulos e a `IQueryable`.
- Testes unitários de revisão usam implementações falsas dos contratos para
  exercitar consentimento, assinatura, responsabilidade e estado da ficha.
- Testes de integração dos repositórios simulam falhas de gravação e conflito
  de versão, verificando que não restem alterações ou auditoria parciais.
- Os testes existentes de API, SQL Server e navegador cobrem o comportamento
  integrado; os de SQL Server exigem a configuração opcional descrita no guia local.

## Consequências e limites

As regras podem ser testadas sem inicializar o EF Core. Em contrapartida,
existem mais interfaces e implementações para manter; elas seguem operações
reais do sistema, sem tentar abstrair toda a API do ORM.

O backend continua em um único projeto .NET, com limites por namespaces
verificados por teste. Isso não equivale a isolamento entre assemblies nem
a independência de todos os componentes em relação ao ASP.NET Core Identity.
A refatoração preserva o esquema, as migrations e os contratos HTTP existentes.

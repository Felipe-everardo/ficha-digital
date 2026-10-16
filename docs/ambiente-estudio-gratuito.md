# Ambiente do estúdio sem serviços pagos

Documento reconstruído em 15/10/2026 com base no histórico da preparação de
13/10/2026. Não é uma recuperação literal do arquivo excluído nem uma nova
consulta de preços ou disponibilidade do Azure.

## Objetivo aceito

Manter uma base de código para duas aplicações independentes: uma vitrine
pública com dados fictícios e um piloto reservado ao estúdio. A evolução do
piloto para atendimentos reais depende das verificações operacionais pendentes.
Cada ambiente tem aplicação, banco e configuração próprios.

## Recursos provisionados na preparação

| Componente | Piloto do estúdio |
| --- | --- |
| Grupo | `rg-fichadigital-estudio` |
| Aplicação | `fichadigital-estudio`, Windows / .NET 10, East US |
| Plano da aplicação | `asp-fichadigital-estudio`, F1 |
| Servidor SQL | `sql-fichadigital-estudio`, West US 2 |
| Banco | `FichaDigitalEstudioDb`, oferta gratuita |
| Autenticação SQL | Microsoft Entra; aplicação com identidade gerenciada |
| Publicação | Manual pelo environment GitHub `estudio`, somente na `main` |

A criação de um servidor SQL em East US foi recusada pelo Azure. O servidor
exclusivo foi criado em West US 2 após confirmação do usuário. Na revisão da
criação, a cobrança por excedente do banco estava desabilitada, com pausa ao
esgotar a cota. Não foram adicionados Application Insights ou serviços pagos.
Esses são registros históricos; conferir custo e cotas no portal antes de
qualquer alteração de plano ou recurso.

## Limites do orçamento

O plano gratuito e o banco com cota não constituem garantia de disponibilidade
contínua. Pausas, retomada do banco e limites de capacidade devem ser considerados
durante os testes e antes de assumir compromissos de atendimento diário.

O estudo e o guia original registraram restrição de destino na restauração
nativa do SQL gratuito: outro banco da mesma oferta gratuita não seria um
destino aceito. Antes de escolher um procedimento de recuperação, conferir as
condições vigentes e demonstrar a restauração em ambiente isolado. Não habilitar
um destino pago sem avaliar custo e obter autorização para essa despesa.

## Alternativas para recuperação a avaliar

- Restauração nativa para destino compatível, com custo conhecido e teste real.
- Exportação ou cópia administrativa para ambiente isolado, verificando
  consistência, segurança, perda de dados aceitável e restauração.
- Backup protegido das chaves de Data Protection, separado da publicação e do
  repositório público. Uma cópia do banco não substitui a recuperação das chaves.

Nenhuma dessas alternativas foi comprovada como rotina operacional nesta
preparação. Não tratar um SQL de migrations como backup dos dados dos clientes.

## Situação registrada em 13/10/2026

A versão `71ee391` foi publicada no piloto. Após corrigir a política de conexão
SQL para Proxy e a senha inicial, o workflow terminou com sucesso e a prontidão
respondeu HTTP 200 / `Healthy`. A conta exclusiva foi criada; o provisionamento
inicial foi desabilitado e sua senha temporária removida da configuração.

Continuam pendentes o teste de login pelo operador, o atendimento fictício
completo, a verificação da separação entre ambientes e os critérios para dados
reais: recuperação, proteção de chaves, acesso, monitoramento e termos.

## Fluxo de evolução

1. Desenvolver e testar localmente.
2. Revisar e integrar as mudanças à `main`; o fluxo existente publica a demo.
3. Validar a demonstração com dados fictícios.
4. Executar manualmente `Publicar piloto do estúdio (manual)` para atualizar o piloto.
5. Tratar alterações do esquema SQL com identidade administrativa separada.

Procedimentos: [publicação do estúdio](publicacao-estudio.md),
[evolução](evolucao.md) e [operação](operacao-producao.md).

Referências preservadas para conferência futura:
[Azure SQL gratuito](https://learn.microsoft.com/azure/azure-sql/database/free-offer)
e [planos do App Service](https://learn.microsoft.com/azure/app-service/overview-hosting-plans).

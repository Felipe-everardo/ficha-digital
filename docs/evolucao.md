# Evolução do portfólio até a operação real

O projeto mantém uma base de código. Configuração, dados e acesso devem ser
separados por ambiente. Esta lista distingue trabalho existente de trabalho
planejado; não comprova a configuração dos recursos no Azure.

## Fase atual — demonstração e avaliação técnica

Implementado no código:

- [x] Fluxo de cadastro, convite, anamnese, consentimento e registro profissional.
- [x] Convites por link e QR Code, com duração de uma hora.
- [x] Novo formato de URL que mantém o token fora do caminho HTTP.
- [x] Filtros pela data de criação e rolagem ao trocar de etapa.
- [x] Testes de domínio, integração e fluxo no navegador.
- [x] Casos de uso sem dependência direta do EF Core, com contratos de persistência
  e teste automatizado das dependências da camada de aplicação.
- [x] Health check que detecta migrations pendentes no SQL Server.
- [x] Documentação de execução, decisões técnicas e roteiro de demonstração.
- [x] Publicação condicionada às verificações de backend, SQL Server e frontend
  do mesmo commit, com cenários E2E independentes.

Próximas melhorias da demonstração:

- [ ] Aviso de dados fictícios dentro da interface, além do README.
- [ ] Rotina controlada para recompor os dados da demonstração.
- [ ] Processo de migrations integrado à publicação, com identidade separada.
- [ ] Recuperação de acesso e revisão do provisionamento da conta inicial.

## Antes do primeiro atendimento real

- [ ] Criar aplicação, banco, identidades e armazenamento de chaves exclusivos
  de produção. A conta pública da demo não deve ter acesso a eles.
- [ ] Definir perda máxima aceitável de dados e tempo de recuperação.
- [ ] Restaurar backup em ambiente isolado e registrar os resultados.
- [ ] Comprovar persistência e proteção das chaves de Data Protection,
  continuidade de sessão e leitura de respostas protegidas após reinício.
- [ ] Validar monitoramento, recuperação e atualização do banco.
- [ ] Definir responsabilidades de operação, acesso, retenção e exportação.
- [ ] Validar os textos e procedimentos com o estúdio e a assessoria responsável.

Critérios e execução: [runbook de produção](operacao-producao.md).

## Direção para os ambientes

| Ambiente | Objetivo | Dados |
| --- | --- | --- |
| Local e testes | Desenvolvimento e validação automatizada | Fictícios; banco temporário no E2E |
| Demonstração | Avaliação pública do portfólio | Fictícios e recuperáveis por carga de demonstração |
| Produção futura | Atendimento do estúdio | Reais, com acesso restrito e recuperação comprovada |

Releases de produção deverão usar uma versão testada e liberação controlada.
Não é necessário criar duas cópias permanentes do código, nem transformar o
banco público de demonstração em banco de produção.

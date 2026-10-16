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

## Melhorias locais de outubro de 2026

- Exportação individual de fichas concluídas em PDF A4 de uma página, com dados
  históricos, termo aceito, assinaturas e nome de arquivo baseado no cliente.
- Acesso autenticado e solicitação de exportação registrada na auditoria.
- Controle para mostrar e ocultar senha com ícone fixo dentro do campo.
- Ajustes mobile: link de retorno do cliente apenas no desktop, cabeçalho com
  nome do estúdio e logo central escura restrita ao início mobile.
- Avisos de demonstração no topo e de sessão ativa removidos a pedido do usuário.
- Dependência System.Security.Cryptography.Pkcs atualizada para 9.0.21; os dois
  alertas conhecidos deixaram de aparecer na consulta NuGet.

Validação: 175 testes de backend aprovados e 4 de SQL Server não executados por
exigirem Docker; fluxos de login, convites e PDFs verificados no navegador.
As últimas telas mobile e desktop foram conferidas com API simulada em porta
separada para preservar os servidores locais em uso. Compilação e lint aprovados.
Estas melhorias ainda não foram publicadas no Azure.

## Piloto do estúdio

Preparação concluída em 13/10/2026 e registro reconstruído em 15/10/2026:

- [x] Aplicação e banco exclusivos do piloto, separados da demonstração.
- [x] Identidades distintas para execução e publicação, com acesso limitado.
- [x] Workflow manual do estúdio integrado à `main`, com validação antes do deploy.
- [x] Perfil Piloto com validação de configuração e migrations automáticas desabilitadas.
- [x] Conta inicial exclusiva criada; provisionamento desabilitado e senha temporária removida.
- [x] Publicação da versão `71ee391` e prontidão verificadas na preparação.
- [ ] Confirmar login e atendimento fictício completo com a profissional.
- [ ] Comprovar separação de dados e credenciais entre demo e piloto.
- [ ] Cumprir os critérios de recuperação, chaves e operação antes dos dados reais.

O item anterior de infraestrutura de produção permanece aberto: provisionar o
piloto não comprova todos os requisitos para atendimentos reais.

Estudo do orçamento: [ambiente do estúdio sem serviços pagos](ambiente-estudio-gratuito.md).
Procedimentos e resultados: [publicação manual do estúdio](publicacao-estudio.md).

## Direção para os ambientes

| Ambiente | Objetivo | Dados |
| --- | --- | --- |
| Local e testes | Desenvolvimento e validação automatizada | Fictícios; banco temporário no E2E |
| Demonstração | Avaliação pública do portfólio | Fictícios e recuperáveis por carga de demonstração |
| Produção futura | Atendimento do estúdio | Reais, com acesso restrito e recuperação comprovada |

Releases de produção deverão usar uma versão testada e liberação controlada.
Não é necessário criar duas cópias permanentes do código, nem transformar o
banco público de demonstração em banco de produção.

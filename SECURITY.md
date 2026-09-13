# Política de segurança

## Estado do projeto

Este é um projeto educacional em desenvolvimento. Ele possui controles de
acesso, antiforgery, rate limiting, auditoria operacional e cabeçalhos
defensivos, mas ainda não passou por auditoria independente de segurança nem
pela validação completa do ambiente de produção.

Não utilize esta versão para armazenar:

- dados pessoais reais;
- documentos de identificação;
- informações de saúde;
- assinaturas;
- credenciais de produção.

## Dados de demonstração

Exemplos, testes e screenshots devem utilizar somente informações fictícias.
Não publique fichas reais em issues, pull requests, commits ou discussões.

## Segredos

Senhas, tokens e connection strings de produção não devem ser adicionados ao
repositório. Em uma implantação futura, esses valores serão fornecidos por
variáveis de ambiente ou por um serviço de gerenciamento de segredos.

As chaves do ASP.NET Data Protection devem ser persistidas em armazenamento
protegido e compartilhado entre instâncias. Logs e ferramentas de suporte não
devem receber corpos de fichas, CPF, respostas clínicas, assinaturas ou links
de convite.

Consulte [`docs/operacao-producao.md`](docs/operacao-producao.md) antes de
publicar uma versão destinada a dados reais.

## Relato de vulnerabilidades

Caso encontre uma vulnerabilidade, não inclua dados pessoais ou instruções de
exploração em uma issue pública. Prefira o recurso privado de relato de
vulnerabilidades do GitHub, quando estiver habilitado no repositório.

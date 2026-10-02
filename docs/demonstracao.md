# Roteiro de demonstração

Este roteiro permite conhecer o fluxo do projeto sem ler todo o código.
Use apenas informações e assinaturas fictícias. O acesso público está no
[README](../README.md#experimente-o-projeto).

## Exploração rápida

1. Entre na área profissional e abra **Clientes**.
2. Clique em **Mostrar todos os clientes**, abra um cadastro e consulte seu
   histórico. Os registros da demo podem mudar conforme outras pessoas a usam.
3. Abra **Histórico**, selecione **Todo o histórico** e clique em **Aplicar**.
   O padrão da tela é mostrar apenas as fichas criadas hoje.
4. Abra uma ficha para conhecer a apresentação dos dados e as seções de
   atendimento, questionário e consentimento.

## Experimentar um atendimento

1. Cadastre um cliente fictício com um nome fácil de identificar.
2. Informe o nome do profissional e selecione tatuagem ou piercing.
3. Gere o convite e abra o link em outra aba, preservando a área profissional.
   Também é possível ler o QR Code no celular.
4. Preencha os campos com dados de teste. O formulário valida idade e formato
   de informações; para uma execução reproduzível, consulte os dados usados no
   [teste E2E](../src/frontend/e2e/fluxo-profissional.spec.ts).
5. Responda ao questionário, confira o resumo, desenhe uma assinatura fictícia
   e autorize o procedimento. O aceite é feito uma única vez.
6. Volte à área profissional, encontre a ficha e faça a revisão.
7. Explore o registro técnico posterior ao procedimento. Seus campos variam
   entre tatuagem e piercing; o registro financeiro faz parte da ficha, não é
   um módulo financeiro independente.

O convite expira uma hora após a geração. Se expirar durante a avaliação,
gere outro convite para continuar com uma nova ficha. O tempo de duração do
procedimento não é limitado pela validade do convite.

## O que observar no código

- **Regra de negócio:** transições e validações no
  [domínio de fichas](../src/backend/FichaDigital.Api/Modules/Fichas/Domain).
- **Integração:** consultas, autenticação e respostas HTTP nos
  [testes de integração](../tests/backend/FichaDigital.IntegrationTests).
- **Frontend:** organização do fluxo no
  [hook da ficha pública](../src/frontend/src/hooks/useFichaPublica.ts).
- **Decisões e limitações:** [ADRs](decisoes) e [plano de evolução](evolucao.md).

## Avaliação local

Siga o [guia de desenvolvimento](desenvolvimento-local.md). Para usar o QR no
celular, ambos os dispositivos precisam estar na mesma rede: inicie o Vite com
`npm --prefix src/frontend run dev -- --host 0.0.0.0` e abra no computador o
endereço de rede informado por ele antes de gerar o convite. Um link com
`localhost` aponta para o próprio dispositivo que o abre.

As capturas do README foram obtidas localmente com dados de teste; não são
mockups nem evidência de operação com clientes reais. A versão publicada pode
ficar atrás da versão do código enquanto uma release não for concluída.

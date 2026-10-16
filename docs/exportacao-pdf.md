# Exportação de uma ficha em PDF

Na área profissional, abrir uma ficha concluída e clicar em **Baixar PDF**.
O arquivo tem uma página A4, com identificação, questionário, termo aceito,
procedimento, pagamento, assinaturas originais e datas no horário de Brasília.

## Dados e acesso

- A API exige a sessão autenticada da conta do estúdio, como as consultas de fichas.
- `GET /api/fichas/{fichaId}/exportacao-pdf` retorna os dados da ficha e o conteúdo
  do termo armazenado no aceite. Não consulta o termo atual para substituir o antigo.
- Os dados pessoais vêm do registro confirmado naquela ficha, mesmo que o cadastro
  do cliente tenha sido atualizado depois.
- O middleware existente grava a solicitação na auditoria com profissional, ficha e
  instante. Esse registro comprova a solicitação; não confirma a gravação no dispositivo.
- Resposta sem cache. O PDF é gerado no navegador com jsPDF e baixado diretamente;
  nenhum serviço externo recebe os dados e nenhum arquivo público é criado no servidor.
- Fichas não concluídas, incompletas ou com evidência cujo hash não confere são recusadas.
- Fichas legadas sem assinatura profissional identificam a ausência, sem inventar assinatura.

## Uma página sem cortes

O gerador mede as quebras de linha e ajusta a fonte entre 9,5 e 8 pontos. Se o
conteúdo ainda ultrapassar a página, informa o problema e interrompe o download.
Não remove observações nem altera o termo para caber. Caracteres sem glifo na
fonte padrão também interrompem a exportação com mensagem, em vez de desaparecerem.
Uma futura ampliação de fontes ou de layout poderá atender esses casos excepcionais.

## Validação

- Testes de integração: autenticação, ficha inexistente, estado incompleto,
  integridade, dados históricos, termo original, cache e auditoria.
- Testes no navegador: fluxo completo para tatuagem e piercing, assinaturas de
  cliente e profissional, download de PDF A4 com uma página, texto maior e recusa
  de conteúdo que não cabe. Arquivos fictícios em `src/frontend/test-results/`.
- Login permite mostrar/ocultar a senha, iniciando oculto e voltando a ocultar
  depois da tentativa de entrada.
- O teste de convites confirma QR code no celular ao criar nova ficha para um
  cliente que já possui convite anterior. Essa funcionalidade já existia no código.

Esta exportação individual não substitui backup e restauração do banco. Exportação
em lote não faz parte desta versão. A publicação na nuvem segue os fluxos existentes;
o estúdio continua com publicação manual.

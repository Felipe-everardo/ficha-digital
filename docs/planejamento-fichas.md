# Planejamento das fichas de tatuagem e piercing

## Objetivo

Este documento consolida o escopo funcional das fichas digitais de tatuagem e
piercing antes das alterações no banco de dados, no backend e no frontend. A
implementação deve substituir as fichas de papel sem misturar novamente o
controle financeiro completo ao núcleo do produto.

As fichas de papel recebidas são a referência de conteúdo. As decisões tomadas
com o estúdio complementam os campos ausentes ou já existentes no sistema.

## Escopo desta evolução

- gerar a ficha correta a partir do procedimento escolhido no convite;
- compartilhar dados pessoais e histórico de saúde entre os procedimentos;
- apresentar o conteúdo específico para tatuagem ou piercing;
- manter dados, anamnese e autorização em uma sequência contínua para o
  cliente;
- impedir a revisão profissional sem autorização registrada;
- registrar a revisão profissional e a conclusão da ficha;
- permitir que o profissional registre os dados técnicos após o procedimento;
- registrar valor, sinal e forma de pagamento vinculados à ficha;
- coletar nome digitado e assinatura desenhada do cliente e do profissional;
- preservar versões e evidências sem alterar fichas antigas.

## Fora do escopo atual

- painel financeiro, despesas, fluxo de caixa e fechamento;
- parcelamento ou divisão entre diferentes formas de pagamento;
- exportação para PDF ou Excel;
- exclusão automática baseada em prazo de retenção;
- configuração administrativa dos modelos de ficha;
- assinatura certificada pela ICP-Brasil;
- uso de CNPJ verdadeiro;
- operação com menores de 18 anos.

## Fluxo da ficha

1. O profissional seleciona o cliente e o tipo de procedimento.
2. O sistema cria a ficha com o modelo e as versões correspondentes.
3. O cliente recebe um convite temporário e preenche dados pessoais e saúde.
4. O termo é apresentado imediatamente depois da anamnese.
5. O cliente revisa os dados e o termo, confirma a leitura e autorização em um
   único checkbox e registra nome e assinatura.
6. O profissional responsável confere a ficha e a identidade do cliente pela
   conta autenticada do estúdio e registra uma única revisão.
7. O procedimento é realizado.
8. O profissional registra os dados técnicos, o pagamento e sua assinatura.
9. A ficha é concluída.

## Estados e transições

| Estado | Significado | Próxima transição permitida |
| --- | --- | --- |
| `Rascunho` | Ficha criada sem convite ativo | Emitir convite |
| `ConviteEnviado` | Convite válido aguardando acesso | Iniciar preenchimento ou expirar |
| `EmPreenchimento` | Cliente iniciou o preenchimento | Concluir anamnese |
| `AnamnesePreenchida` | Dados pessoais e saúde foram preenchidos | Cliente revisar o termo e assinar |
| `AguardandoConsentimento` | Estado legado, mantido somente para fichas do fluxo anterior | Cliente revisar o termo e assinar |
| `AutorizadaParaProcedimento` | Consentimento pré-procedimento está íntegro e bloqueado | Profissional revisar |
| `RevisadaPeloProfissional` | Ficha e identidade foram conferidas; registro posterior ainda está pendente | Registrar dados pós-procedimento |
| `Concluida` | Registro técnico, pagamento e assinatura profissional concluídos | Nenhuma alteração comum |
| `Expirada` | Convite expirou antes da autorização | Emitir uma nova ficha |
| `Cancelada` | Ficha cancelada com justificativa | Nenhuma alteração comum |

A interface não deve apresentar apenas “ficha incompleta” durante o
procedimento. Ela deve mostrar separadamente que o consentimento está concluído
e que o registro profissional ainda está pendente.

## Identificação da ficha

| Campo | Responsável | Momento | Regra |
| --- | --- | --- | --- |
| Cliente | Profissional | Criação | Obrigatório |
| Profissional responsável | Operador do estúdio | Criação | Nome completo obrigatório e preservado na ficha |
| Tipo do procedimento | Profissional | Criação | `Tatuagem` ou `Piercing` |
| Versão do modelo | Sistema | Criação | Imutável após emissão do convite |
| CNPJ apresentado | Sistema | Criação | Usar temporariamente `00.000.000/0000-00` |
| Data de criação | Sistema | Criação | UTC |

O CNPJ deve vir de uma configuração central do estúdio. A ficha deve guardar o
valor apresentado naquele momento para que uma futura troca de configuração
não reescreva o histórico.

## Dados pessoais comuns

| Campo | Aplicação | Obrigatoriedade | Regra |
| --- | --- | --- | --- |
| Nome completo | Ambas | Obrigatório | Nome civil usado na identificação e assinatura |
| Nome social | Ambas | Opcional | Preferido para exibição quando informado |
| Pronomes | Ambas | Opcional | Texto curto |
| Estado civil | Ambas | Obrigatório | Mantido porque integra os dois termos fornecidos |
| Data de nascimento | Ambas | Obrigatório | Deve comprovar idade mínima de 18 anos |
| CPF | Ambas | Obrigatório | Armazenar 11 dígitos e validar verificadores |
| Celular | Ambas | Obrigatório | Armazenar normalizado e exibir com máscara |
| Telefone adicional | Ambas | Opcional | Mantém o campo existente na ficha de piercing |
| E-mail | Ambas | Opcional | Validar formato quando informado |
| Instagram | Ambas | Opcional | Normalizar o identificador quando informado |
| Contato de emergência nome | Ambas | Opcional | Nome e celular devem ser informados juntos |
| Contato de emergência celular | Ambas | Opcional | Nome e celular devem ser informados juntos |
| CEP | Ambas | Obrigatório | Parte do endereço declarado |
| Logradouro | Ambas | Obrigatório | Parte do endereço declarado |
| Número | Ambas | Obrigatório | Aceitar “sem número” de forma explícita |
| Complemento | Ambas | Opcional | Texto curto |
| Bairro | Ambas | Obrigatório | Parte do endereço declarado |
| Cidade | Ambas | Obrigatório | Parte do endereço declarado |
| Estado | Ambas | Obrigatório | Unidade federativa |

Os dados confirmados devem continuar sendo copiados para a ficha. Alterações no
cadastro geral do cliente não podem modificar fichas antigas.

## Questionário de saúde comum

Todas as perguntas são obrigatórias e aceitam `Sim` ou `Não`. O detalhe
condicional é obrigatório quando a resposta correspondente for `Sim`.

| Pergunta | Complemento condicional |
| --- | --- |
| Tem diabetes? | Tipo de diabetes |
| Teve anemia? | Tipo ou detalhes sobre a anemia |
| Teve hepatite? | Tipo de hepatite |
| Possui pressão alta? | Nenhum |
| Possui alguma condição cardíaca? | Nenhum |
| Tem epilepsia? | Nenhum |
| Tem hemofilia? | Nenhum |
| Possui doença transmissível? | Qual doença |
| Tem alergia? | Descrição e observações |
| Usa marca-passo? | Nenhum |
| Fuma? | Nenhum |
| Consumiu bebida alcoólica nas últimas 24 horas? | Nenhum |
| Faz uso de medicação? | Qual medicação |
| Está grávida ou amamentando? | Nenhum |

O novo questionário deve receber uma nova versão. Registros das versões
anteriores permanecem somente para leitura.

## Conteúdo pré-procedimento

O termo resolvido para tatuagem ou piercing contém as declarações específicas
do procedimento. Esses itens não devem ser repetidos como uma lista de
checkboxes. Depois de revisar os dados, a anamnese e o texto completo, o cliente
registra um único aceite de leitura e autorização, acompanhado do nome completo
e da assinatura desenhada.

## Consentimento do cliente

O consentimento somente pode ser registrado quando os dados pessoais e o
histórico de saúde estiverem completos.

| Evidência | Regra |
| --- | --- |
| Nome do cliente digitado | Obrigatório e compatível com o nome confirmado |
| Assinatura desenhada | Obrigatória |
| Versão e conteúdo do termo | Preservar cópia exata |
| Respostas do questionário | Preservar cópia exata |
| Confirmação de leitura e autorização | Obrigatória e única |
| Tipo do procedimento | Preservar cópia exata |
| Data e hora do aceite | Gerada pelo servidor em UTC |
| Dados técnicos mínimos do acesso | Registrar conforme política de privacidade |
| Código de integridade | Calcular sobre toda a evidência preservada |

Depois do aceite, dados pessoais, respostas, preparação e consentimento não
podem ser editados pelo fluxo comum. Uma correção administrativa futura deve
ser auditável e não apagar a versão anterior.

## Revisão profissional

O comando de revisão deve exigir:

- ficha em `AutorizadaParaProcedimento`;
- consentimento íntegro;
- conta do estúdio autenticada;
- uma única confirmação explícita de que os dados da ficha e a identidade do
  cliente foram conferidos;
- registro do profissional e do horário da revisão.

O profissional não precisa armazenar o número do RG para registrar que realizou
a conferência presencial da identidade.

## Registro posterior de tatuagem

| Campo | Obrigatoriedade |
| --- | --- |
| Arte efetivamente tatuada | Obrigatória |
| Material utilizado | Obrigatório |
| Local efetivo da tatuagem | Obrigatório |
| Observações do procedimento | Opcional |
| Valor total | Obrigatório |
| Valor do sinal | Obrigatório, aceitando zero |
| Forma de pagamento | `Pix`, `Dinheiro` ou `Cartao` |
| Nome profissional digitado | Obrigatório |
| Assinatura profissional desenhada | Obrigatória |

## Registro posterior de piercing

| Campo | Obrigatoriedade |
| --- | --- |
| Tipo de joia utilizada | Obrigatório |
| Agulha utilizada | Obrigatória |
| Local efetivo da perfuração | Obrigatório |
| Observações do procedimento | Opcional |
| Valor total | Obrigatório |
| Valor do sinal | Obrigatório, aceitando zero |
| Forma de pagamento | `Pix`, `Dinheiro` ou `Cartao` |
| Nome profissional digitado | Obrigatório |
| Assinatura profissional desenhada | Obrigatória |

O valor restante deve ser calculado como `valor total - sinal` e não precisa ser
persistido. Valores negativos e sinal superior ao total são inválidos.

## Regras de assinatura

- o cliente informa o nome e desenha a assinatura;
- o profissional desenha a assinatura; seu nome é reutilizado da ficha para
  evitar divergência por redigitação;
- a conta autenticada identifica quem executou a operação no sistema;
- o arquivo ou os traços da assinatura devem fazer parte do cálculo de
  integridade;
- não permitir substituir a assinatura depois do bloqueio da respectiva etapa;
- o fluxo digital completo será submetido a revisão jurídica antes da produção.

## Estratégia de modelagem

Manter um núcleo tipado e versionado, sem criar um construtor genérico de
formulários neste momento:

```text
Ficha
├── DadosPessoaisFicha
├── QuestionarioSaude
├── AceiteTermoConsentimento
├── RevisaoProfissional
├── RegistroProcedimento
│   ├── RegistroTatuagem
│   └── RegistroPiercing
```

Os termos devem ser resolvidos pelo backend de acordo com o tipo da ficha. O
frontend apenas apresenta o modelo retornado, e o backend rejeita respostas
incompatíveis com o tipo registrado.

## Plano de implementação

### Etapa 1 Regras e estados do domínio

- [x] ampliar `StatusFicha` e suas transições;
- [x] separar conclusão da anamnese, autorização, revisão profissional e
  conclusão técnica;
- [x] criar testes unitários para as transições válidas e inválidas;
- [x] manter leitura das fichas antigas por uma transição de compatibilidade
  explicitamente identificada no domínio.

### Etapa 2 Dados pessoais e saúde

- [x] acrescentar estado civil, CPF, telefone adicional e endereço ao retrato
  da ficha;
- [x] atualizar cadastro reutilizável do cliente sem reescrever o histórico;
- [x] ampliar o questionário de saúde e criar a versão 3;
- [x] implementar validações condicionais e testes;
- [x] criar migration compatível com os registros antigos depois dos testes do
  domínio.

### Etapa 3 Modelos e termos por procedimento

- [x] apresentar o conteúdo específico de tatuagem e piercing no termo;
- [x] criar um resolvedor de modelo pelo tipo da ficha;
- [x] versionar separadamente modelo, questionário e termo;
- [x] usar CNPJ fictício vindo de configuração;
- [x] preservar o conteúdo apresentado em cada aceite.

### Etapa 4 Fluxo público

- [x] manter dados, anamnese e consentimento em um único fluxo público;
- [x] apresentar somente campos compatíveis com o procedimento;
- [x] adicionar revisão final, nome digitado e assinatura desenhada;
- [x] bloquear o convite após o consentimento;
- [x] testar retomada, expiração e tentativas de alteração.

### Etapa 5 Operação profissional

- [x] entregar a ficha autorizada diretamente para conferência do profissional;
- [x] permitir confirmar a revisão somente com autorização válida;
- [x] criar registro posterior específico por procedimento;
- [x] registrar valor, sinal e forma de pagamento;
- [x] adicionar nome e assinatura profissional;

### Etapa 6 Consultas e experiência de uso

- [x] mostrar separadamente o progresso do cliente e do profissional;
- [x] atualizar listagens, filtros e detalhe da ficha;
- [x] destacar fichas autorizadas, revisadas e com registro pendente;
- [x] impedir que uma ficha autorizada apareça apenas como “incompleta”.

### Etapa 7 Qualidade e preparação da demonstração

- [x] ampliar testes unitários e de integração para os dois procedimentos;
- [x] validar autorização e integridade antes da revisão profissional;
- [x] revisar segurança e exposição de dados sensíveis;
- [x] atualizar scripts de demonstração e documentação;
- [x] executar o fluxo completo de tatuagem e piercing.

## Ajustes solicitados após o teste manual

Os itens abaixo substituem decisões de experiência adotadas na primeira
implementação do fluxo. A implementação está sendo realizada em partes; os
itens concluídos estão marcados.

### Fluxo simplificado entre cliente e profissional

Fluxo desejado:

1. O profissional informa o nome do cliente, escolhe o procedimento e envia o
   convite.
2. O cliente preenche os dados pessoais e de saúde, lê o termo, informa o nome
   completo, desenha a assinatura e conclui tudo em uma única sequência.
3. A ficha preenchida fica disponível para revisão do profissional, sem exigir
   que ele libere uma nova etapa para o cliente.
4. O profissional confere os dados e registra uma única confirmação.
5. Depois do procedimento, o profissional informa os dados técnicos, os valores
   e a forma de pagamento e conclui a ficha.

- [x] eliminar a alternância de liberação entre as telas do cliente e do
  profissional;
- [x] permitir que o cliente conclua todo o preenchimento e o aceite sem
  aguardar uma ação intermediária do profissional;
- [x] criar uma única revisão e confirmação profissional antes do
  procedimento;
- [x] revisar os estados, transições, endpoints e textos da interface para o
  novo fluxo;
- [x] manter o registro técnico e financeiro como etapa posterior ao
  procedimento.

### Simplificação do aceite do cliente

- [x] remover as confirmações específicas por checkbox de tatuagem e piercing;
- [x] remover os checkboxes separados de maioridade, dados pessoais e
  questionário de saúde;
- [x] continuar bloqueando menores de 18 anos pela data de nascimento, sem
  exigir uma confirmação redundante por checkbox;
- [x] manter somente um checkbox com a declaração de que o cliente leu e
  entendeu o texto apresentado;
- [x] manter o nome completo digitado e a assinatura desenhada;
- [x] preservar na evidência a versão e o conteúdo exibidos, os dados
  preenchidos, o instante do aceite e a assinatura, mesmo com a interface
  simplificada;
- [x] simplificar o resumo do aceite, apresentando os dados técnicos de
  integridade como informação secundária ou em uma área de detalhes.

### Registro profissional

- [x] retirar a data prevista de retorno do registro de tatuagem em todo o
  backend, frontend, banco e testes;
- [x] retirar o registro de interrupções e todas as estruturas associadas no
  domínio, API, persistência, frontend, migrations, documentação e testes;
- [x] formatar os campos monetários como Real brasileiro durante a digitação,
  mostrando `R$` e separadores adequados e convertendo o valor com segurança
  antes do envio;
- [x] substituir a mensagem genérica de sucesso por mensagens específicas para
  cada ação;
- [x] limpar a mensagem da ação anterior quando o estado da ficha mudar, para
  que a confirmação da revisão não pareça confirmar antecipadamente o registro
  técnico posterior.

### Listagem e localização das fichas

- [x] ao abrir a aba `Fichas` sem nenhum filtro, exibir automaticamente as
  fichas mais recentes do dia;
- [x] ordenar a listagem padrão da mais recente para a mais antiga, usando o
  horário de criação ou envio do convite;
- [x] mostrar na listagem informações que ajudem o profissional a localizar
  rapidamente o atendimento: nome do cliente, tipo de procedimento, horário e
  situação atual da ficha;
- [x] ao aplicar uma busca ou filtro, substituir a listagem padrão pelos
  resultados filtrados; ao limpar os filtros, voltar às fichas recentes do dia;
- [x] exibir um estado vazio claro quando ainda não houver fichas criadas no
  dia.

## Decisões ainda pendentes

- revisar com o estúdio a redação final do complemento sobre anemia;
- revisar o mecanismo de aceite e assinatura digital antes da produção;
- informar o CNPJ verdadeiro antes da produção;
- definir retenção, exportação e backup em evolução futura.

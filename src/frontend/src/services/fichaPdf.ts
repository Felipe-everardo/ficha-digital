import { jsPDF } from 'jspdf'
import type { ExportacaoFicha } from './fichasApi'

const dataHora = (valor: string) => new Intl.DateTimeFormat('pt-BR', {
  dateStyle: 'short', timeStyle: 'short', timeZone: 'America/Sao_Paulo',
}).format(new Date(valor))
const moeda = (valor: number) => valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const texto = (valor: string | null | undefined) => valor?.trim() || '-'
const resposta = (valor: boolean, detalhe?: string | null) => `${valor ? 'Sim' : 'Não'}${detalhe ? ` - ${detalhe}` : ''}`

// Toda a ficha é medida novamente em cada tamanho. Nunca cortamos texto para fazê-la caber.
export function criarPdfFicha(dados: ExportacaoFicha): jsPDF {
  for (const tamanho of [9.5, 9, 8.5, 8]) {
    const { pdf, fim } = desenharFicha(dados, tamanho)
    if (fim <= 794) return pdf
  }
  throw new Error('O conteúdo desta ficha excede uma folha A4. O PDF não foi baixado para evitar cortes; será necessário ajustar o modelo de exportação.')
}

function desenharFicha({ ficha: f, conteudoTermo, exportadaEmUtc }: ExportacaoFicha, tamanho: number) {
  if (!f.aceiteTermo || !f.questionarioSaude) throw new Error('A ficha não possui consentimento e questionário completos.')
  const pdf = new jsPDF({ unit: 'pt', format: 'a4', compress: true })
  pdf.setProperties({ title: `Ficha de atendimento ${f.id}`, author: 'Manuscrito Estúdio' })
  const c = f.cliente
  const q = f.questionarioSaude
  const a = f.aceiteTermo
  const r = f.registroProcedimento
  const margem = 32
  const largura = pdf.internal.pageSize.getWidth() - margem * 2
  const entrelinha = tamanho * 1.25
  let y = 36

  function linhas(valor: string, larguraTexto: number): string[] {
    // Helvetica inclui os caracteres do português. Caracteres sem glifo não podem sumir no documento.
    // eslint-disable-next-line no-control-regex -- Permite somente os controles de espaço e glifos suportados.
    if (/[^\u0009\u000a\u000d\u0020-\u00ff\u2013\u2014\u2018\u2019\u201c\u201d\u2022\u2026\u20ac]/u.test(valor)) {
      throw new Error('Esta ficha contém caracteres que a fonte do PDF ainda não suporta. Nenhuma informação foi removida; o download foi interrompido.')
    }
    return pdf.splitTextToSize(valor.replace(/\s+/g, ' ').trim(), larguraTexto) as string[]
  }
  function estilo(negrito = false, fonte = tamanho) {
    pdf.setFont('helvetica', negrito ? 'bold' : 'normal')
    pdf.setFontSize(fonte)
    pdf.setTextColor(35, 42, 34)
  }
  function secao(titulo: string) {
    y += 8
    pdf.setFillColor(233, 238, 228)
    pdf.rect(margem, y - 10, largura, 18, 'F')
    estilo(true)
    pdf.text(titulo, margem + 6, y + 2)
    y += 19
  }
  function paragrafo(valor: string) {
    estilo()
    const partes = linhas(valor, largura - 8)
    pdf.text(partes, margem + 4, y, { lineHeightFactor: 1.25 })
    y += partes.length * entrelinha + 3
  }
  function grade(campos: [string, string][]) {
    estilo()
    const coluna = (largura - 14) / 2
    for (let i = 0; i < campos.length; i += 2) {
      let altura = 0
      for (let j = 0; j < 2 && i + j < campos.length; j++) {
        const [rotulo, valor] = campos[i + j]
        const partes = linhas(`${rotulo}: ${valor}`, coluna - 8)
        pdf.text(partes, margem + 4 + j * (coluna + 14), y, { lineHeightFactor: 1.25 })
        altura = Math.max(altura, partes.length * entrelinha)
      }
      y += altura + 4
    }
  }
  estilo(true, 18)
  pdf.text('Manuscrito Estúdio', margem, y)
  estilo(true, 11)
  pdf.text('FICHA DE ATENDIMENTO', margem + largura, y, { align: 'right' })
  y += 18
  estilo(false, 8)
  pdf.text(`Ficha ${f.id} | ${f.tipoProcedimento} | Concluída`, margem, y)
  y += 5
  secao('DADOS DO CLIENTE')
  const nascimento = c.dataNascimento ? c.dataNascimento.split('-').reverse().join('/') : '-'
  grade([
    ['Nome completo', texto(c.nomeCompleto)], ['Nome social', texto(c.nomeSocial)],
    ['Nascimento', nascimento], ['CPF', texto(c.cpf)],
    ['Celular', texto(c.celular)], ['Telefone adicional', texto(c.telefoneAdicional)],
    ['E-mail', texto(c.email)], ['Instagram', texto(c.instagram)],
    ['Estado civil', texto(c.estadoCivil)], ['Pronomes', texto(c.pronomes)],
  ])
  paragrafo(`Endereço: ${[c.logradouro, c.numero, c.complemento, c.bairro, c.cidade, c.estado, c.cep].filter(Boolean).join(', ') || '-'}`)
  paragrafo(`Emergência: ${texto(c.contatoEmergenciaNome)} | ${texto(c.contatoEmergenciaCelular)}`)
  secao(`QUESTIONÁRIO DE SAÚDE - VERSÃO ${q.versao}`)
  grade([
    ['Diabetes', resposta(q.temDiabetes, q.tipoDiabetes)], ['Pressão alta', resposta(q.possuiPressaoAlta)],
    ['Anemia', resposta(q.teveAnemia, q.descricaoAnemia)], ['Hepatite', resposta(q.teveHepatite, q.tipoHepatite)],
    ['Alergia', resposta(q.temAlergia, q.descricaoAlergia)], ['Condição cardíaca', resposta(q.possuiCondicaoCardiaca)],
    ['Epilepsia', resposta(q.temEpilepsia)], ['Hemofilia', resposta(q.temHemofilia)],
    ['Doença transmissível', resposta(q.possuiDoencaTransmissivel, q.descricaoDoencaTransmissivel)], ['Marca-passo', resposta(q.usaMarcaPasso)],
    ['Fuma', resposta(q.fuma)], ['Álcool nas últimas 24h', resposta(q.consumiuBebidaAlcoolicaUltimas24Horas)],
    ['Medicação', resposta(q.usaMedicacao, q.descricaoMedicacao)], ['Grávida ou amamentando', resposta(q.estaGravidaOuAmamentando)],
  ])
  secao(`TERMO DE CONSENTIMENTO - VERSÃO ${a.versaoTermo}`)
  paragrafo(conteudoTermo)
  secao('PROCEDIMENTO E RESPONSÁVEL')
  paragrafo(`Profissional responsável: ${f.profissionalResponsavelNome} | Procedimento: ${f.tipoProcedimento}`)
  if (r) {
    if (r.tipoProcedimento === 'Tatuagem') {
      paragrafo(`Arte: ${texto(r.arteEfetivamenteTatuada)} | Local: ${texto(r.localTatuagem)} | Material: ${texto(r.materialUtilizado)}`)
    } else {
      paragrafo(`Joia: ${texto(r.joiaUtilizada)} | Local: ${texto(r.localPerfuracao)} | Agulha: ${texto(r.agulhaUtilizada)}`)
    }
    if (r.observacoes) paragrafo(`Observações: ${r.observacoes}`)
    paragrafo(`Total: ${moeda(r.valorTotal)} | Sinal: ${moeda(r.valorSinal)} | Restante: ${moeda(r.valorRestante)} | ${r.formaPagamento}`)
  }
  if (f.cnpjApresentado) paragrafo(`CNPJ apresentado: ${f.cnpjApresentado}`)
  secao('ASSINATURAS')
  function assinatura(imagem: string | null, nome: string, papel: string, instante: string, x: number) {
    const coluna = largura / 2 - 12
    if (imagem) {
      if (!imagem.startsWith('data:image/png;base64,')) throw new Error('Formato de assinatura não suportado para exportação.')
      const props = pdf.getImageProperties(imagem)
      const escala = Math.min(coluna / props.width, 36 / props.height)
      pdf.addImage(imagem, 'PNG', x, y, props.width * escala, props.height * escala)
    }
    pdf.setDrawColor(150)
    pdf.line(x, y + 38, x + coluna, y + 38)
    estilo(false, tamanho - 0.5)
    const nomes = linhas(nome, coluna)
    pdf.text(nomes, x, y + 50, { lineHeightFactor: 1.25 })
    const altura = nomes.length * entrelinha
    pdf.text(`${papel} | ${dataHora(instante)}`, x, y + 50 + altura)
    return 55 + altura + entrelinha
  }
  const clienteAltura = assinatura(a.assinaturaDesenhada, a.nomeAssinante, 'Cliente', a.aceitoEmUtc, margem + 4)
  const profissionalAltura = r ? assinatura(r.assinaturaDesenhada, r.nomeProfissionalAssinante, 'Profissional', r.registradoEmUtc, margem + largura / 2 + 8) : 0
  if (!r) {
    estilo(false, 8)
    pdf.text('Assinatura profissional não registrada nesta ficha.', margem + largura / 2 + 8, y + 20)
  }
  y += Math.max(clienteAltura, profissionalAltura) + 8
  estilo(false, 7.5)
  pdf.text(`Criada: ${dataHora(f.criadaEmUtc)} | Questionário: ${dataHora(q.respondidoEmUtc)}${f.revisaoProfissional ? ` | Revisada: ${dataHora(f.revisaoProfissional.revisadaEmUtc)}` : ''}`, margem, y)
  y += 12
  pdf.setDrawColor(190)
  pdf.line(margem, 805, margem + largura, 805)
  pdf.text(`Exportada em ${dataHora(exportadaEmUtc)} (horário de Brasília)`, margem, 818)
  pdf.text('1 / 1', margem + largura, 818, { align: 'right' })
  return { pdf, fim: y }
}

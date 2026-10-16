import { readFile } from 'node:fs/promises'
import { expect, test } from './fixtures'
import { autenticar, criarCliente, emitirConvite, caminhoConvite, nomeCliente } from './apoio'

for (const tipo of ['Piercing', 'Tatuagem'] as const) {
test(`cliente preenche, assina e exporta ${tipo} em uma página`, async ({ page }) => {
  await autenticar(page.request)
  const cliente = await criarCliente(page.request, nomeCliente('preenchimento'))
  const convite = await emitirConvite(page.request, cliente.id, tipo)
  await page.context().clearCookies()
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto(caminhoConvite(convite))
  // Todas as transições começam no topo, inclusive depois de formulários longos.
  for (const [campo, valor] of Object.entries({
    'Nome completo *': 'Cliente do teste E2E',
    'Estado civil *': 'Solteiro',
    'Data de nascimento *': '01/01/1990',
    'CPF *': '52998224725',
    'Celular / WhatsApp *': '11999999999',
    'CEP *': '01001000',
    'Logradouro *': 'Rua de Teste',
    'Número *': '100',
    'Bairro *': 'Centro',
    'Cidade *': 'São Paulo',
    'Estado (UF) *': 'SP',
  })) {
    await page.getByRole('textbox', { name: campo }).fill(valor)
  }
  await page.getByRole('button', { name: 'Salvar e continuar' }).scrollIntoViewIfNeeded()
  expect(await page.evaluate(() => window.scrollY)).toBeGreaterThan(100)
  await page.getByRole('button', { name: 'Salvar e continuar' }).click()
  await expect(page.getByRole('heading', { name: 'Histórico de saúde' })).toBeVisible()
  await expect.poll(() => page.evaluate(() => window.scrollY)).toBe(0)
  await expect(page.getByRole('heading', { name: 'Sua ficha digital' })).toBeFocused()

  // Validação na mesma etapa não deve mandar a pessoa de volta ao início.
  await page.getByRole('button', { name: 'Salvar e continuar' }).click()
  await expect(page.getByRole('alert')).toHaveText('Responda todas as perguntas antes de continuar.')
  expect(await page.evaluate(() => window.scrollY)).toBeGreaterThan(100)
  for (const resposta of await page.getByRole('radio', { name: 'Não', exact: true }).all()) {
    await resposta.check()
  }
  await page.getByRole('button', { name: 'Salvar e continuar' }).click()
  await expect(page.getByRole('heading', { name: 'Termo de consentimento', exact: true })).toBeVisible()
  await expect.poll(() => page.evaluate(() => window.scrollY)).toBe(0)

  const assinatura = page.getByLabel('Área para desenhar a assinatura')
  await assinatura.scrollIntoViewIfNeeded()
  const area = (await assinatura.boundingBox())!
  await page.mouse.move(area.x + 20, area.y + 35)
  await page.mouse.down()
  await page.mouse.move(area.x + 80, area.y + 60, { steps: 12 })
  await page.mouse.move(area.x + 140, area.y + 30, { steps: 12 })
  await page.mouse.up()
  await page.getByRole('checkbox').check()
  await page.getByRole('button', { name: 'Assinar e autorizar procedimento' }).click()
  await expect(page.getByRole('heading', { name: 'Procedimento autorizado com segurança.' })).toBeVisible()
  await expect.poll(() => page.evaluate(() => window.scrollY)).toBe(0)

  await autenticar(page.request)
  await page.goto(`/profissional/fichas/${convite.fichaId}`)
  await expect(page.getByRole('button', { name: 'Baixar PDF' })).toHaveCount(0)
  await page.getByRole('checkbox', { name: 'Conferi a ficha completa' }).check()
  await page.getByRole('button', { name: 'Confirmar revisão profissional' }).click()
  const camposTecnicos = tipo === 'Piercing' ? {
    'Joia utilizada *': 'Titânio grau implante',
    'Agulha utilizada *': 'Cateter descartável',
    'Local efetivo da perfuração *': 'Lóbulo esquerdo',
  } : {
    'Arte efetivamente tatuada *': 'Ramo botânico',
    'Material utilizado *': 'Agulhas e tinta de teste',
    'Local efetivo da tatuagem *': 'Antebraço esquerdo',
  }
  for (const [campo, valor] of Object.entries({
    ...camposTecnicos,
    'Valor total *': '15000',
    'Valor do sinal *': '5000',
  })) await page.getByRole('textbox', { name: campo, exact: true }).fill(valor!)
  await page.getByLabel('Forma de pagamento *').selectOption('Pix')
  const profissional = page.getByLabel('Área para desenhar a assinatura')
  await profissional.scrollIntoViewIfNeeded()
  const areaProfissional = (await profissional.boundingBox())!
  await page.mouse.move(areaProfissional.x + 20, areaProfissional.y + 35)
  await page.mouse.down()
  await page.mouse.move(areaProfissional.x + 120, areaProfissional.y + 65, { steps: 15 })
  await page.mouse.up()
  await page.getByRole('button', { name: 'Salvar registro e concluir ficha' }).click()
  await expect(page.getByRole('button', { name: 'Baixar PDF' })).toBeVisible()
  const downloadPromise = page.waitForEvent('download')
  await page.getByRole('button', { name: 'Baixar PDF' }).click()
  const download = await downloadPromise
  expect(download.suggestedFilename()).toMatch(/^Ficha - Cliente do teste E2E - \d{4}-\d{2}-\d{2}\.pdf$/)
  await download.saveAs(`test-results/ficha-exportada-${tipo.toLowerCase()}.pdf`)
  const bytes = await readFile((await download.path())!)
  expect(bytes.subarray(0, 5).toString()).toBe('%PDF-')
  expect(bytes.toString('latin1').match(/\/Type \/Page\b/g)).toHaveLength(1)

  // Textos maiores devem caber inteiros; conteúdo excessivo não pode gerar um arquivo cortado.
  const dadosResponse = await page.request.get(`/api/fichas/${convite.fichaId}/exportacao-pdf`)
  expect(dadosResponse.status()).toBe(200)
  const dados = await dadosResponse.json()
  await page.evaluate(async (exportacao) => {
    const caminhoModulo = '/src/services/fichaPdf.ts'
    const { criarPdfFicha } = await import(/* @vite-ignore */ caminhoModulo)
    exportacao.ficha.registroProcedimento.observacoes = 'Observação extensa para verificar a quebra de linhas. '.repeat(8)
    const pdf = criarPdfFicha(exportacao)
    if (pdf.getNumberOfPages() !== 1) throw new Error('PDF deve ter uma página')
    exportacao.conteudoTermo = 'Texto extenso '.repeat(5000)
    let recusou = false
    try { criarPdfFicha(exportacao) } catch { recusou = true }
    if (!recusou) throw new Error('Não pode exportar texto cortado')
  }, dados)
})
}

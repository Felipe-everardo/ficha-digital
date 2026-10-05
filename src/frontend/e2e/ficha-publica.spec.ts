import { expect, test } from './fixtures'
import { autenticar, criarCliente, emitirConvite, caminhoConvite, nomeCliente } from './apoio'

test('cliente preenche, valida e assina; cada etapa começa no topo', async ({ page }) => {
  await autenticar(page.request)
  const cliente = await criarCliente(page.request, nomeCliente('preenchimento'))
  const convite = await emitirConvite(page.request, cliente.id, 'Piercing')
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
})

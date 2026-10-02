import { expect, test } from '@playwright/test'

test('conta do estúdio entra, cadastra cliente e gera convite', async ({ page }) => {
  test.setTimeout(60_000)
  await page.goto('/profissional')

  await page.getByLabel('E-mail').fill('proprietaria.e2e@example.com')
  await page.getByLabel('Senha').fill('Teste-E2E-Segura-123!')
  await page.getByRole('button', { name: 'Entrar' }).click()

  await expect(page.getByRole('heading', {
    name: 'Bem-vindo ao Manuscrito Studio.',
  }))
    .toBeVisible()
  await expect(page.getByText('Conta do estúdio', { exact: true })).toBeVisible()

  await page.goto('/profissional/clientes/novo')
  await page.getByLabel('Nome para identificar o cliente *')
    .fill('Cliente do teste E2E')
  await page.getByRole('button', { name: 'Cadastrar cliente' }).click()

  await expect(page.getByText('Cliente do teste E2E foi cadastrado.'))
    .toBeVisible()
  await page.getByLabel('Profissional responsável *')
    .fill('Lia Tatuadora')
  await page.getByLabel('Procedimento *').selectOption('Tatuagem')
  await page.getByRole('button', { name: 'Gerar convite agora' }).click()

  await expect(page.getByRole('heading', { name: /Compartilhe o convite/ }))
    .toBeVisible()
  await expect(page.getByLabel('Link de preenchimento'))
    .toHaveValue(/\/fichas\/preencher#/)
  await expect(page.getByRole('img', { name: 'QR Code para preencher a ficha' }))
    .toBeVisible()
  const link = await page.getByLabel('Link de preenchimento').inputValue()
  const token = new URL(link).hash.slice(1)
  const urlsRequisitadas: string[] = []
  page.on('request', request => urlsRequisitadas.push(request.url()))
  await page.goto(link)
  await expect(page.getByText('Lia Tatuadora', { exact: true })).toBeVisible()
  await expect(page.getByText('Tatuagem', { exact: true })).toBeVisible()
  await expect(page).toHaveURL(/\/fichas\/preencher$/)
  expect(urlsRequisitadas.every(url => !url.includes(token))).toBe(true)
  await page.reload()
  await expect(page.getByText('Lia Tatuadora', { exact: true })).toBeVisible()

  // Links emitidos antes da atualização continuam funcionando.
  await page.goto(link.replace('#', '/'))
  await expect(page.getByText('Lia Tatuadora', { exact: true })).toBeVisible()
  await expect(page).toHaveURL(/\/fichas\/preencher$/)

  // A lista de clientes também compartilha convites, inclusive no celular.
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/profissional/clientes')
  await page.getByRole('button', { name: 'Mostrar todos os clientes' }).click()
  await page.getByRole('button', { name: 'Nova ficha', exact: true }).click()
  await page.getByLabel('Profissional responsável *').fill('Thais Piercer')
  await page.getByLabel('Qual procedimento será realizado?').selectOption('Piercing')
  await page.getByRole('button', { name: 'Gerar convite', exact: true }).click()
  const qr = page.getByRole('img', { name: 'QR Code para preencher a ficha' })
  await expect(qr).toBeVisible()
  const bounds = await qr.boundingBox()
  expect(bounds).not.toBeNull()
  expect(bounds!.x).toBeGreaterThanOrEqual(0)
  expect(bounds!.x + bounds!.width).toBeLessThanOrEqual(390)
  const novoLink = await page.getByLabel('Link de preenchimento').inputValue()
  expect(novoLink).not.toBe(link)
  expect(new URL(novoLink).pathname).toBe('/fichas/preencher')
  expect(new URL(novoLink).hash.length).toBeGreaterThan(1)
  await page.goto(novoLink)
  await expect(page.getByText('Thais Piercer', { exact: true })).toBeVisible()
  await expect(page.getByText('Piercing', { exact: true })).toBeVisible()

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

  // Filtrar a data de criação inclui fichas que ainda não foram concluídas.
  const agora = new Date()
  const ano = String(agora.getFullYear())
  const mes = String(agora.getMonth() + 1).padStart(2, '0')
  const dia = String(agora.getDate()).padStart(2, '0')
  await page.goto('/profissional/fichas')
  await expect(page.locator('tbody tr')).toHaveCount(2)
  for (const [tipo, campo, valor] of [
    ['dia', 'Dia da criação', `${dia}/${mes}/${ano}`],
    ['mes', 'Mês da criação', `${mes}/${ano}`],
    ['ano', 'Ano da criação', ano],
  ]) {
    await page.getByRole('combobox', { name: 'Tipo', exact: true }).selectOption(tipo)
    await page.getByLabel(campo).fill(valor)
    const resposta = page.waitForResponse(response => response.url().includes('/api/fichas?'))
    await page.getByRole('button', { name: 'Aplicar', exact: true }).click()
    const consulta = new URL((await resposta).url())
    expect(consulta.searchParams.has('criadaDe')).toBe(true)
    expect(consulta.searchParams.has('concluidaDe')).toBe(false)
    await expect(page.locator('tbody tr')).toHaveCount(2)
  }
  await page.getByLabel('Ano da criação').fill('2000')
  await page.getByRole('button', { name: 'Aplicar', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Nenhuma ficha encontrada' })).toBeVisible()
  await page.getByRole('combobox', { name: 'Tipo', exact: true }).selectOption('todos')
  await page.getByRole('button', { name: 'Aplicar', exact: true }).click()
  await expect(page.locator('tbody tr')).toHaveCount(2)
  await page.getByRole('button', { name: 'Voltar para hoje' }).click()
  await expect(page.getByRole('combobox', { name: 'Tipo', exact: true })).toHaveValue('hoje')
  await expect(page.locator('tbody tr')).toHaveCount(2)
})

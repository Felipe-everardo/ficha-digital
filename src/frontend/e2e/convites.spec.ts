import { expect, test } from './fixtures'
import { autenticar, criarCliente, emitirConvite, caminhoConvite, nomeCliente } from './apoio'

test('convite por fragmento, retomada e compatibilidade com links antigos', async ({ page }) => {
  await autenticar(page.request)
  const cliente = await criarCliente(page.request, nomeCliente('links'))
  const convite = await emitirConvite(page.request, cliente.id)
  const link = new URL(caminhoConvite(convite), 'http://127.0.0.1:5173').href
  await page.context().clearCookies()
  const token = new URL(link).hash.slice(1)
  const urlsRequisitadas: string[] = []
  page.on('request', request => urlsRequisitadas.push(request.url()))
  await page.goto(link)
  await expect(page.getByText('Profissional E2E', { exact: true })).toBeVisible()
  await expect(page.getByText('Tatuagem', { exact: true })).toBeVisible()
  await expect(page).toHaveURL(/\/fichas\/preencher$/)
  expect(urlsRequisitadas.every(url => !url.includes(token))).toBe(true)
  await page.reload()
  await expect(page.getByText('Profissional E2E', { exact: true })).toBeVisible()

  // Links emitidos antes da atualização continuam funcionando.
  await page.goto(link.replace('#', '/'))
  await expect(page.getByText('Profissional E2E', { exact: true })).toBeVisible()
  await expect(page).toHaveURL(/\/fichas\/preencher$/)
})

test('novo convite pela lista de clientes com QR Code no celular', async ({ page }) => {
  await autenticar(page.request)
  const nome = nomeCliente('celular')
  const cliente = await criarCliente(page.request, nome)
  const anterior = await emitirConvite(page.request, cliente.id)
  // A lista de clientes também compartilha convites, inclusive no celular.
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/profissional/clientes')
  await page.getByLabel('Nome do cliente').fill(nome)
  await page.getByRole('button', { name: 'Buscar clientes' }).click()
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
  expect(new URL(novoLink).hash).not.toBe(new URL(anterior.linkPreenchimento, novoLink).hash)
  expect(new URL(novoLink).pathname).toBe('/fichas/preencher')
  expect(new URL(novoLink).hash.length).toBeGreaterThan(1)
  await page.goto(novoLink)
  await expect(page.getByText('Thais Piercer', { exact: true })).toBeVisible()
  await expect(page.getByText('Piercing', { exact: true })).toBeVisible()
})

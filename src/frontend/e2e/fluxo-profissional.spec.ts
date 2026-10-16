import { expect, test } from './fixtures'
import { nomeCliente } from './apoio'

test('login, cadastro e convite com QR Code', async ({ page }) => {
  const nome = nomeCliente('cadastro')
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/profissional')

  await page.getByLabel('E-mail').fill('proprietaria.e2e@example.com')
  await page.getByLabel('Senha', { exact: true }).fill('Teste-E2E-Segura-123!')
  await expect(page.getByLabel('Senha', { exact: true })).toHaveAttribute('type', 'password')
  await expect(page.getByRole('complementary', { name: 'Ambiente de demonstração' })).toHaveCount(0)
  const campoSenha = (await page.getByLabel('Senha', { exact: true }).boundingBox())!
  const olho = (await page.getByRole('button', { name: 'Mostrar senha' }).boundingBox())!
  expect(olho.x).toBeGreaterThan(campoSenha.x)
  expect(olho.x + olho.width).toBeLessThanOrEqual(campoSenha.x + campoSenha.width)
  expect(olho.y).toBeGreaterThanOrEqual(campoSenha.y)
  expect(olho.y + olho.height).toBeLessThanOrEqual(campoSenha.y + campoSenha.height)
  await page.getByRole('button', { name: 'Mostrar senha' }).hover()
  await page.waitForTimeout(200)
  const olhoDepoisDoHover = (await page.getByRole('button', { name: 'Mostrar senha' }).boundingBox())!
  expect(olhoDepoisDoHover.x).toBe(olho.x)
  expect(olhoDepoisDoHover.y).toBe(olho.y)
  await page.screenshot({ path: 'test-results/login-olho.png', fullPage: true })
  await page.getByRole('button', { name: 'Mostrar senha' }).click()
  await expect(page.getByLabel('Senha', { exact: true })).toHaveAttribute('type', 'text')
  await expect(page.getByLabel('Senha', { exact: true })).toHaveValue('Teste-E2E-Segura-123!')
  await page.getByRole('button', { name: 'Ocultar senha' }).click()
  await expect(page.getByLabel('Senha', { exact: true })).toHaveAttribute('type', 'password')
  await page.setViewportSize({ width: 1280, height: 720 })
  await page.getByRole('button', { name: 'Entrar', exact: true }).click()

  await expect(page.getByRole('heading', {
    name: 'Bem-vindo ao Manuscrito Studio.',
  }))
    .toBeVisible()
  await expect(page.getByText('Conta do estúdio', { exact: true })).toBeVisible()

  await page.goto('/profissional/clientes/novo')
  await page.getByLabel('Nome para identificar o cliente *')
    .fill(nome)
  await page.getByRole('button', { name: 'Cadastrar cliente' }).click()

  await expect(page.getByText(`${nome} foi cadastrado.`))
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
})

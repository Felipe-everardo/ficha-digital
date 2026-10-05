import { expect, test } from './fixtures'
import { nomeCliente } from './apoio'

test('login, cadastro e convite com QR Code', async ({ page }) => {
  const nome = nomeCliente('cadastro')
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

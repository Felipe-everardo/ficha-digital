import { expect, test } from '@playwright/test'

test('conta do estúdio entra, cadastra cliente e gera convite', async ({ page }) => {
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

  await expect(page.getByRole('heading', { name: /Envie este link/ }))
    .toBeVisible()
  await expect(page.getByLabel('Link de preenchimento'))
    .toHaveValue(/\/fichas\/preencher\//)
  const link = await page.getByLabel('Link de preenchimento').inputValue()
  await page.goto(link)
  await expect(page.getByText('Lia Tatuadora', { exact: true })).toBeVisible()
  await expect(page.getByText('Tatuagem', { exact: true })).toBeVisible()
})

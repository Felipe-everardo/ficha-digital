import { expect, test } from './fixtures'
import { autenticar, criarCliente, emitirConvite, nomeCliente } from './apoio'

test('histórico filtra por criação e inclui fichas ainda não concluídas', async ({ page }) => {
  await autenticar(page.request)
  const cliente = await criarCliente(page.request, nomeCliente('filtros'))
  const primeira = await emitirConvite(page.request, cliente.id)
  const segunda = await emitirConvite(page.request, cliente.id, 'Piercing')
  async function verificarFichasCriadas() {
    // Verifica os IDs deste cenário, sem depender do total deixado por outros testes.
    for (const convite of [primeira, segunda]) {
      await expect(page.locator(
        `tbody a[href="/profissional/fichas/${convite.fichaId}"]`,
      )).toBeVisible()
    }
  }
  // Filtrar a data de criação inclui fichas que ainda não foram concluídas.
  const agora = new Date()
  const ano = String(agora.getFullYear())
  const mes = String(agora.getMonth() + 1).padStart(2, '0')
  const dia = String(agora.getDate()).padStart(2, '0')
  await page.goto('/profissional/fichas')
  await verificarFichasCriadas()
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
    await verificarFichasCriadas()
  }
  await page.getByLabel('Ano da criação').fill('2000')
  await page.getByRole('button', { name: 'Aplicar', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Nenhuma ficha encontrada' })).toBeVisible()
  await page.getByRole('combobox', { name: 'Tipo', exact: true }).selectOption('todos')
  await page.getByRole('button', { name: 'Aplicar', exact: true }).click()
  await verificarFichasCriadas()
  await page.getByRole('button', { name: 'Voltar para hoje' }).click()
  await expect(page.getByRole('combobox', { name: 'Tipo', exact: true })).toHaveValue('hoje')
  await verificarFichasCriadas()
})

import { randomUUID } from 'node:crypto'
import { expect, type APIRequestContext } from '@playwright/test'
import type { ClienteCriado } from '../src/services/clientesApi'
import type { ConviteFichaCriado } from '../src/services/fichasApi'

export function nomeCliente(cenario: string) {
  return `E2E ${cenario} ${randomUUID()}`
}

async function enviar(request: APIRequestContext, url: string, data: unknown) {
  const tokenResponse = await request.get('/api/autenticacao/antiforgery-token')
  expect(tokenResponse.status()).toBe(200)
  const { token } = await tokenResponse.json()
  return request.post(url, {
    data,
    headers: {
      'X-CSRF-TOKEN': token,
      'Idempotency-Key': randomUUID(),
    },
  })
}

// Prepara pré-condições pela API real; o cenário de login testa a interface.
export async function autenticar(request: APIRequestContext) {
  const response = await enviar(request, '/api/autenticacao/entrar', {
    email: 'proprietaria.e2e@example.com',
    senha: 'Teste-E2E-Segura-123!',
  })
  expect(response.status()).toBe(200)
}

export async function criarCliente(request: APIRequestContext, nome: string) {
  const response = await enviar(request, '/api/clientes', { nomeReferencia: nome })
  expect(response.status()).toBe(201)
  return await response.json() as ClienteCriado
}

export async function emitirConvite(
  request: APIRequestContext,
  clienteId: string,
  tipoProcedimento: 'Tatuagem' | 'Piercing' = 'Tatuagem',
) {
  const response = await enviar(request, `/api/clientes/${clienteId}/fichas/convites`, {
    profissionalResponsavelNome: 'Profissional E2E',
    tipoProcedimento,
  })
  expect(response.status()).toBe(201)
  return await response.json() as ConviteFichaCriado
}

// Aceita a URL relativa da API e mantém a navegação na origem do frontend.
export function caminhoConvite(convite: ConviteFichaCriado) {
  const url = new URL(convite.linkPreenchimento, 'http://127.0.0.1:5173')
  return `${url.pathname}${url.hash}`
}

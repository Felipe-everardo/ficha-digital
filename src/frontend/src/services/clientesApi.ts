import type { FichaClienteResumo, TipoProcedimento } from './fichasApi'
import { adicionarFiltros, ApiRequestError, ApiValidationError } from './http'

export type CriarClienteInput = {
  nomeReferencia: string
}

export type ClienteCriado = {
  id: string
  nomeParaExibicao: string
  criadoEmUtc: string
}

export type ClienteResumo = {
  id: string
  nomeReferencia: string
  nomeCompleto: string | null
  nomeParaExibicao: string
  pronomes: string | null
  celular: string | null
  email: string | null
  instagram: string | null
  criadoEmUtc: string
  ultimaFicha: FichaClienteResumo | null
}

export type FiltrosClientes = {
  busca?: string
  profissionalId?: string
  tipoProcedimento?: TipoProcedimento
  ultimaFichaDe?: string
  ultimaFichaAte?: string
}

export type ClientesPaginados = {
  itens: ClienteResumo[]
  pagina: number
  tamanhoPagina: number
  totalItens: number
  totalPaginas: number
}

export type ClienteDetalhe = {
  id: string
  nomeReferencia: string
  nomeCompleto: string | null
  nomeSocial: string | null
  nomeParaExibicao: string
  pronomes: string | null
  dataNascimento: string | null
  celular: string | null
  email: string | null
  instagram: string | null
  contatoEmergenciaNome: string | null
  contatoEmergenciaCelular: string | null
  dadosPessoaisPreenchidosEmUtc: string | null
  criadoEmUtc: string
  fichas: FichaClienteResumo[]
}

export async function criarCliente(
  cliente: CriarClienteInput,
  antiforgeryToken: string,
): Promise<ClienteCriado> {
  const response = await fetch('/api/clientes', {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': antiforgeryToken,
    },
    body: JSON.stringify(cliente),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as {
      errors?: Record<string, string[]>
    }

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    throw new Error('Não foi possível concluir o cadastro.')
  }

  return response.json() as Promise<ClienteCriado>
}

export async function listarClientes(
  pagina: number,
  tamanhoPagina: number,
  filtros: FiltrosClientes = {},
  signal?: AbortSignal,
): Promise<ClientesPaginados> {
  const parametros = new URLSearchParams({
    pagina: pagina.toString(),
    tamanhoPagina: tamanhoPagina.toString(),
  })
  adicionarFiltros(parametros, filtros)
  const response = await fetch(`/api/clientes?${parametros}`, {
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : 'Não foi possível consultar os clientes.',
    )
  }

  return response.json() as Promise<ClientesPaginados>
}

export async function obterDetalheCliente(
  clienteId: string,
  signal?: AbortSignal,
): Promise<ClienteDetalhe> {
  const response = await fetch(
    `/api/clientes/${encodeURIComponent(clienteId)}`,
    {
      credentials: 'same-origin',
      signal,
    },
  )

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : response.status === 404
          ? 'O cliente informado não foi encontrado.'
          : 'Não foi possível consultar o cliente.',
    )
  }

  return response.json() as Promise<ClienteDetalhe>
}

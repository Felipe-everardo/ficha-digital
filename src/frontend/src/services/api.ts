export type ApiStatus = {
  application: string
  message: string
  version: string
  checkedAtUtc: string
}

export type CriarClienteInput = {
  nomeCompleto: string
  nomeSocial: string
  pronomes: string
  dataNascimento: string
  celular: string
  email: string
}

export type ClienteCriado = {
  id: string
  nomeParaExibicao: string
  pronomes: string | null
  criadoEmUtc: string
}

export type ValidationProblemDetails = {
  title: string
  status: number
  errors: Record<string, string[]>
}

export class ApiValidationError extends Error {
  readonly errors: Record<string, string[]>

  constructor(errors: Record<string, string[]>) {
    super('A API encontrou erros de validação.')
    this.name = 'ApiValidationError'
    this.errors = errors
  }
}

export async function getApiStatus(): Promise<ApiStatus> {
  const response = await fetch('/api/status')

  if (!response.ok) {
    throw new Error('A API respondeu com um erro.')
  }

  return response.json() as Promise<ApiStatus>
}

export async function criarCliente(
  cliente: CriarClienteInput,
): Promise<ClienteCriado> {
  const response = await fetch('/api/clientes', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      ...cliente,
      email: cliente.email.trim() || null,
    }),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    throw new Error('Não foi possível concluir o cadastro.')
  }

  return response.json() as Promise<ClienteCriado>
}

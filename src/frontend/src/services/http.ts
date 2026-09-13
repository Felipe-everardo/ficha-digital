export type ProblemDetails = {
  title?: string
  detail?: string
  status?: number
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

export class ApiRequestError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiRequestError'
    this.status = status
  }
}

export async function verificarErroValidacao(response: Response) {
  if (response.status !== 400) return

  const problem = (await response.json()) as ValidationProblemDetails

  if (problem.errors) {
    throw new ApiValidationError(problem.errors)
  }
}

export async function criarErroDaApi(
  response: Response,
  mensagemPadrao: string,
) {
  let problem: ProblemDetails | null = null

  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
    // Algumas falhas de infraestrutura podem não retornar JSON.
  }

  return new ApiRequestError(
    response.status,
    problem?.detail ?? problem?.title ?? mensagemPadrao,
  )
}

export function adicionarFiltros(
  parametros: URLSearchParams,
  filtros: Record<string, string | boolean | undefined>,
) {
  Object.entries(filtros).forEach(([chave, valor]) => {
    if (valor !== undefined && valor !== '') {
      parametros.set(chave, String(valor))
    }
  })
}

export function criarChaveIdempotencia() {
  return crypto.randomUUID()
}

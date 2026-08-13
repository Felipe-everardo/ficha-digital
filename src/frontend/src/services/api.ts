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

export type TermoConsentimento = {
  versao: number
  conteudo: string
  conteudoHash: string
}

export type ConviteFichaAberto = {
  fichaId: string
  status: string
  termoConsentimento: TermoConsentimento
}

export type ResponderQuestionarioSaudeInput = {
  temDiabetes: boolean
  tipoDiabetes: string | null
  possuiPressaoAlta: boolean
  temAlergia: boolean
  descricaoAlergia: string | null
  possuiCondicaoCardiaca: boolean
  temEpilepsia: boolean
  temHemofilia: boolean
  usaMarcaPasso: boolean
  estaGravidaOuAmamentando: boolean
}

export type QuestionarioSaudeRespondido = {
  questionarioId: string
  fichaId: string
  versao: number
  respondidoEmUtc: string
}

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

export async function abrirConviteFicha(
  token: string,
  signal?: AbortSignal,
): Promise<ConviteFichaAberto> {
  const response = await fetch('/api/fichas/convites/abrir', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ token }),
    signal,
  })

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível validar este convite.',
    )
  }

  return response.json() as Promise<ConviteFichaAberto>
}

export async function responderQuestionarioSaude(
  token: string,
  respostas: ResponderQuestionarioSaudeInput,
): Promise<QuestionarioSaudeRespondido> {
  const response = await fetch('/api/fichas/questionario-saude', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ token, ...respostas }),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível salvar o questionário.',
    )
  }

  return response.json() as Promise<QuestionarioSaudeRespondido>
}

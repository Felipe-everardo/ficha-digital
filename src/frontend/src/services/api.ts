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

export type ClienteResumo = {
  id: string
  nomeCompleto: string
  nomeParaExibicao: string
  pronomes: string | null
  celular: string
  email: string | null
  criadoEmUtc: string
}

export type ClientesPaginados = {
  itens: ClienteResumo[]
  pagina: number
  tamanhoPagina: number
  totalItens: number
  totalPaginas: number
}

export type ConviteFichaCriado = {
  fichaId: string
  conviteId: string
  linkPreenchimento: string
  expiraEmUtc: string
}

export type FichaResumo = {
  id: string
  clienteId: string
  clienteNome: string
  status: string
  criadaEmUtc: string
  conviteExpiraEmUtc: string | null
  conviteExpirado: boolean
}

export type FichasPaginadas = {
  itens: FichaResumo[]
  pagina: number
  tamanhoPagina: number
  totalItens: number
  totalPaginas: number
}

export type ClienteFichaDetalhe = {
  id: string
  nomeCompleto: string
  nomeSocial: string | null
  nomeParaExibicao: string
  pronomes: string | null
  dataNascimento: string
  celular: string
  email: string | null
}

export type QuestionarioSaudeDetalhe = {
  versao: number
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
  respondidoEmUtc: string
}

export type AceiteTermoResumo = {
  versaoTermo: number
  nomeAssinante: string
  aceitoEmUtc: string
}

export type FichaDetalhe = {
  id: string
  status: string
  criadaEmUtc: string
  conviteExpiraEmUtc: string | null
  conviteExpirado: boolean
  cliente: ClienteFichaDetalhe
  questionarioSaude: QuestionarioSaudeDetalhe | null
  aceiteTermo: AceiteTermoResumo | null
}

export type TermoConsentimento = {
  versao: number
  conteudo: string
  conteudoHash: string
}

export type ConviteFichaAberto = {
  fichaId: string
  status: string
  questionarioRespondido: boolean
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

export type AceitarTermoConsentimentoInput = {
  versaoTermo: number
  conteudoHash: string
  nomeAssinante: string
  aceitouTermo: boolean
}

export type TermoConsentimentoAceito = {
  aceiteId: string
  fichaId: string
  versaoTermo: number
  aceitoEmUtc: string
  statusFicha: string
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
  antiforgeryToken: string,
): Promise<ClienteCriado> {
  const response = await fetch('/api/clientes', {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': antiforgeryToken,
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

export async function listarClientes(
  pagina: number,
  tamanhoPagina: number,
  signal?: AbortSignal,
): Promise<ClientesPaginados> {
  const parametros = new URLSearchParams({
    pagina: pagina.toString(),
    tamanhoPagina: tamanhoPagina.toString(),
  })
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

export async function emitirConviteFicha(
  clienteId: string,
  antiforgeryToken: string,
): Promise<ConviteFichaCriado> {
  const response = await fetch(
    `/api/clientes/${encodeURIComponent(clienteId)}/fichas/convites`,
    {
      method: 'POST',
      credentials: 'same-origin',
      headers: {
        'X-CSRF-TOKEN': antiforgeryToken,
      },
    },
  )

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
        'Não foi possível gerar o convite.',
    )
  }

  return response.json() as Promise<ConviteFichaCriado>
}

export async function listarFichas(
  pagina: number,
  tamanhoPagina: number,
  signal?: AbortSignal,
): Promise<FichasPaginadas> {
  const parametros = new URLSearchParams({
    pagina: pagina.toString(),
    tamanhoPagina: tamanhoPagina.toString(),
  })
  const response = await fetch(`/api/fichas?${parametros}`, {
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : 'Não foi possível consultar as fichas.',
    )
  }

  return response.json() as Promise<FichasPaginadas>
}

export async function obterDetalheFicha(
  fichaId: string,
  signal?: AbortSignal,
): Promise<FichaDetalhe> {
  const response = await fetch(
    `/api/fichas/${encodeURIComponent(fichaId)}`,
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
          ? 'A ficha informada não foi encontrada.'
          : 'Não foi possível consultar os detalhes da ficha.',
    )
  }

  return response.json() as Promise<FichaDetalhe>
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

export async function aceitarTermoConsentimento(
  token: string,
  aceite: AceitarTermoConsentimentoInput,
): Promise<TermoConsentimentoAceito> {
  const response = await fetch('/api/fichas/termo-consentimento/aceitar', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ token, ...aceite }),
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
        'Não foi possível registrar o aceite do termo.',
    )
  }

  return response.json() as Promise<TermoConsentimentoAceito>
}

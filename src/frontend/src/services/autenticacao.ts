import { ApiRequestError, type ProblemDetails } from './api'

export type SessaoProfissional = {
  profissionalId: string
  nomeCompleto: string
  email: string
}

type AntiforgeryTokenResponse = {
  token: string
}

export async function obterAntiforgeryToken(): Promise<string> {
  const response = await fetch('/api/autenticacao/antiforgery-token', {
    credentials: 'same-origin',
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      'Não foi possível preparar a operação segura.',
    )
  }

  const resultado = (await response.json()) as AntiforgeryTokenResponse

  return resultado.token
}

async function criarErroDaApi(
  response: Response,
  mensagemPadrao: string,
): Promise<ApiRequestError> {
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

export async function obterSessaoProfissional(
  signal?: AbortSignal,
): Promise<SessaoProfissional | null> {
  const response = await fetch('/api/autenticacao/sessao', {
    credentials: 'same-origin',
    signal,
  })

  if (response.status === 401) {
    return null
  }

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
      'Não foi possível verificar sua sessão.',
    )
  }

  return response.json() as Promise<SessaoProfissional>
}

export async function entrarProfissional(
  email: string,
  senha: string,
): Promise<SessaoProfissional> {
  const antiforgeryToken = await obterAntiforgeryToken()
  const response = await fetch('/api/autenticacao/entrar', {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': antiforgeryToken,
    },
    body: JSON.stringify({ email, senha }),
  })

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
      'Não foi possível entrar na área profissional.',
    )
  }

  return response.json() as Promise<SessaoProfissional>
}

export async function sairProfissional(): Promise<void> {
  const antiforgeryToken = await obterAntiforgeryToken()
  const response = await fetch('/api/autenticacao/sair', {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      'X-CSRF-TOKEN': antiforgeryToken,
    },
  })

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
      'Não foi possível encerrar sua sessão.',
    )
  }
}

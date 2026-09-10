import { ApiRequestError } from './http'

export type ProfissionalResumo = {
  id: string
  nomeCompleto: string
  especialidades: string[]
}

export async function listarProfissionais(
  signal?: AbortSignal,
): Promise<ProfissionalResumo[]> {
  const response = await fetch('/api/profissionais', {
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : 'Não foi possível consultar os profissionais.',
    )
  }

  return response.json() as Promise<ProfissionalResumo[]>
}

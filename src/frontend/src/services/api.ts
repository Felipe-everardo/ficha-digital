export type ApiStatus = {
  application: string
  message: string
  version: string
  checkedAtUtc: string
}

export type CriarClienteInput = {
  nomeCompleto: string
  dataNascimento: string
  celular: string
}

export type ClienteCriado = {
  id: string
  nomeParaExibicao: string
  pronomes: string | null
  criadoEmUtc: string
}

export async function getApiStatus(): Promise<ApiStatus> {
  const response = await fetch('/api/status')

  if (!response.ok) {
    throw new Error('A API respondeu com um erro.')
  }

  return response.json() as Promise<ApiStatus>
}

export async function criarCliente(cliente: CriarClienteInput, ): Promise<ClienteCriado> {
  const response = await fetch('/api/clientes', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(cliente),
  })

  if (!response.ok) {
    throw new Error('Não foi possível concluir o cadastro.')
  }

  return response.json() as Promise<ClienteCriado>
}

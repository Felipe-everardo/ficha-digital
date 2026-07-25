export type ApiStatus = {
  application: string
  message: string
  version: string
  checkedAtUtc: string
}

export async function getApiStatus(): Promise<ApiStatus> {
  const response = await fetch('/api/status')

  if (!response.ok) {
    throw new Error('A API respondeu com um erro.')
  }

  return response.json() as Promise<ApiStatus>
}

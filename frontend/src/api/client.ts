import { keycloak } from '../features/auth/keycloak'

const baseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

// A failed refresh must not break public calls. Protected calls get a 401 from the API instead.
async function getToken() {
  if (!keycloak.authenticated) return undefined
  return keycloak.updateToken(30).then(
    () => keycloak.token,
    () => undefined,
  )
}

async function request(path: string, init?: RequestInit) {
  const headers = new Headers(init?.headers)
  const token = await getToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${baseUrl}${path}`, { ...init, headers })
  if (!response.ok) {
    let payload: unknown = undefined
    const contentType = response.headers.get('content-type') ?? ''
    if (contentType.includes('application/json')) {
      payload = await response.json().catch(() => undefined)
    }

    const error = new Error(
      `${init?.method ?? 'GET'} ${path} failed with status ${response.status}`,
    ) as Error & { status?: number; payload?: unknown }
    error.status = response.status
    error.payload = payload
    throw error
  }
  return response
}

export async function apiGet<T>(path: string): Promise<T> {
  const response = await request(path)
  return response.json() as Promise<T>
}

export async function apiPost<T = void>(path: string, body: unknown): Promise<T> {
  const response = await request(path, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })

  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('application/json')) {
    return response.json() as Promise<T>
  }

  return undefined as T
}

export async function apiPut<T>(path: string, body: unknown): Promise<T> {
  const response = await request(path, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })

  return response.json() as Promise<T>
}

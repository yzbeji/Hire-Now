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
    throw new Error(
      `${init?.method ?? 'GET'} ${path} failed with status ${response.status}`,
    )
  }
  return response
}

export async function apiGet<T>(path: string): Promise<T> {
  const response = await request(path)
  return response.json() as Promise<T>
}

export async function apiPost(path: string, body: unknown): Promise<void> {
  await request(path, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
}

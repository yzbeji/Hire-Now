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

export async function apiGet<T>(path: string): Promise<T> {
  const headers = new Headers()
  const token = await getToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${baseUrl}${path}`, { headers })
  if (!response.ok) {
    throw new Error(`GET ${path} failed with status ${response.status}`)
  }
  return response.json() as Promise<T>
}

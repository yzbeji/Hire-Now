const baseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

export async function apiGet<T>(path: string): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`)
  if (!response.ok) {
    throw new Error(`GET ${path} failed with status ${response.status}`)
  }
  return response.json() as Promise<T>
}

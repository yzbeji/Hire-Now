import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, expect, it, vi } from 'vitest'
import { keycloak } from '../features/auth/keycloak'
import { HomePage } from './HomePage'

vi.mock('../features/auth/keycloak', () => ({
  keycloak: {
    authenticated: false,
    token: 'test-token',
    updateToken: vi.fn().mockResolvedValue(false),
  },
  keycloakReady: Promise.resolve(),
}))

function renderHomePage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  render(
    <QueryClientProvider client={queryClient}>
      <HomePage />
    </QueryClientProvider>,
  )
}

// Mimics the API: /me needs a token, and /health doesn't.
function stubApi(me: unknown = {}) {
  const fetchMock = vi.fn(async (url: string, init: { headers: Headers }) => {
    const path = new URL(url).pathname
    if (path === '/health') return Response.json({ status: 'Healthy' })
    return init.headers.has('Authorization')
      ? Response.json(me)
      : new Response(null, { status: 401 })
  })
  vi.stubGlobal('fetch', fetchMock)
  return (path: string) =>
    fetchMock.mock.calls.find(([url]) => url.endsWith(path))?.[1].headers
}

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
  keycloak.authenticated = false
  vi.mocked(keycloak.updateToken).mockResolvedValue(false)
})

it('shows the status the API reports', async () => {
  stubApi()

  renderHomePage()

  await screen.findByText('API: Healthy')
})

it('shows the API as unreachable when the request fails', async () => {
  vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('offline')))

  renderHomePage()

  await screen.findByText('API: unreachable')
})

it('offers sign-in and sends no token when signed out', async () => {
  const headersFor = stubApi()

  renderHomePage()

  await screen.findByRole('button', { name: 'Sign in' })
  await screen.findByText('API: Healthy')
  expect(headersFor('/health')?.has('Authorization')).toBe(false)
  expect(headersFor('/me')).toBeUndefined()
})

it('shows the signed-in user and sends the token to the API', async () => {
  keycloak.authenticated = true
  const headersFor = stubApi({ username: 'seller', roles: ['Seller'] })

  renderHomePage()

  await screen.findByText('Signed in as seller (Seller)')
  await screen.findByRole('button', { name: 'Sign out' })
  expect(headersFor('/me')?.get('Authorization')).toBe('Bearer test-token')
})

it('keeps public calls working and offers sign-in again when the token refresh fails', async () => {
  keycloak.authenticated = true
  vi.mocked(keycloak.updateToken).mockRejectedValue(new Error('expired'))
  stubApi()

  renderHomePage()

  await screen.findByText('API: Healthy')
  await screen.findByRole('alert')
  await screen.findByRole('button', { name: 'Sign in again' })
})

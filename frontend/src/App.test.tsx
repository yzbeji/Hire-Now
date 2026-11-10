import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, expect, it, vi } from 'vitest'
import { keycloak } from './features/auth/keycloak'
import { routes } from './routes'

vi.mock('./features/auth/keycloak', () => ({
  keycloak: {
    authenticated: false,
    token: 'test-token',
    updateToken: vi.fn().mockResolvedValue(false),
    login: vi.fn(),
    register: vi.fn(),
  },
  keycloakReady: Promise.resolve(),
}))

function renderApp(path = '/') {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  const router = createMemoryRouter(routes, { initialEntries: [path] })
  render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}

// Mimics the API: /me and /me/role need a token, and /health doesn't.
function stubApi(roles: string[] = [], postStatus = 204) {
  const fetchMock = vi.fn(async (url: string, init: RequestInit) => {
    const path = new URL(url).pathname
    if (path === '/health') return Response.json({ status: 'Healthy' })
    if (!(init.headers as Headers).has('Authorization')) {
      return new Response(null, { status: 401 })
    }
    if (path === '/me/role') {
      if (postStatus === 204) roles = [JSON.parse(String(init.body)).role]
      return new Response(null, { status: postStatus })
    }
    return Response.json({ username: 'sam', roles })
  })
  vi.stubGlobal('fetch', fetchMock)
  return (path: string) =>
    fetchMock.mock.calls.find(([url]) => url.endsWith(path))?.[1]
}

function signIn() {
  keycloak.authenticated = true
}

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
  vi.clearAllMocks()
  keycloak.authenticated = false
  vi.mocked(keycloak.updateToken).mockResolvedValue(false)
})

it('shows the status the API reports', async () => {
  stubApi()

  renderApp()

  await screen.findByText('API: Healthy')
})

it('shows the API as unreachable when the request fails', async () => {
  vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('offline')))

  renderApp()

  await screen.findByText('API: unreachable')
})

it('offers sign-in and registration, and sends no token, when signed out', async () => {
  const requestFor = stubApi()

  renderApp()

  await screen.findByRole('button', { name: 'Sign in' })
  fireEvent.click(screen.getByRole('button', { name: 'Register' }))
  expect(keycloak.register).toHaveBeenCalled()
  await screen.findByText('API: Healthy')
  const headers = requestFor('/health')?.headers as Headers
  expect(headers.has('Authorization')).toBe(false)
  expect(requestFor('/me')).toBeUndefined()
})

it('shows the signed-in user and sends the token to the API', async () => {
  signIn()
  const requestFor = stubApi(['Seller'])

  renderApp()

  const account = await screen.findByRole('link', {
    name: 'Signed in as sam (Seller)',
  })
  expect(account.getAttribute('href')).toBe('/profile')
  await screen.findByRole('button', { name: 'Sign out' })
  const headers = requestFor('/me')?.headers as Headers
  expect(headers.get('Authorization')).toBe('Bearer test-token')
})

it('keeps public calls working and offers sign-in again when the token refresh fails', async () => {
  signIn()
  vi.mocked(keycloak.updateToken).mockRejectedValue(new Error('expired'))
  stubApi()

  renderApp()

  await screen.findByText('API: Healthy')
  await screen.findByRole('alert')
  await screen.findByRole('button', { name: 'Sign in again' })
})

it('shows each role only its own area in the navigation', async () => {
  signIn()
  stubApi(['Buyer'])

  renderApp()

  await screen.findByRole('link', { name: 'Buyer area' })
  expect(screen.queryByRole('link', { name: 'Seller area' })).toBeNull()
  expect(screen.queryByRole('link', { name: 'Admin area' })).toBeNull()
})

it('opens a page for the matching role', async () => {
  signIn()
  stubApi(['Seller'])

  renderApp('/seller')

  await screen.findByRole('heading', { name: 'Seller area' })
})

it.each([
  ['a signed-in user with another role', ['Buyer'], '/seller'],
  ['a seller on the admin page', ['Seller'], '/admin'],
])('sends %s back to the home page', async (_, roles, path) => {
  signIn()
  stubApi(roles)

  renderApp(path)

  await screen.findByRole('heading', { name: 'Find someone for the job' })
})

it('sends a signed-out visitor away from a protected page', async () => {
  stubApi()

  renderApp('/seller')

  await screen.findByRole('heading', { name: 'Find someone for the job' })
  expect(screen.queryByRole('heading', { name: 'Seller area' })).toBeNull()
})

it('asks a new user to choose, saves the role and refreshes the token', async () => {
  signIn()
  const requestFor = stubApi([])

  renderApp()

  await screen.findByRole('heading', { name: 'How will you use HireNow?' })
  expect(screen.queryByRole('button', { name: /admin/i })).toBeNull()
  fireEvent.click(screen.getByRole('button', { name: /Service provider/ }))

  await screen.findByRole('link', { name: 'Seller area' })
  expect(requestFor('/me/role')?.body).toBe('{"role":"Seller"}')
  expect(keycloak.updateToken).toHaveBeenCalledWith(-1)
  expect(
    screen.queryByRole('heading', { name: 'How will you use HireNow?' }),
  ).toBeNull()
})

it('explains when the choice could not be saved', async () => {
  signIn()
  stubApi([], 500)

  renderApp()

  fireEvent.click(await screen.findByRole('button', { name: /Service seeker/ }))

  await screen.findByText("Your choice wasn't saved. Try again.")
})

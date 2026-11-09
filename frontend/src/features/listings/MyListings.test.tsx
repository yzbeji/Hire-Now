import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import {
  cleanup,
  fireEvent,
  render,
  screen,
  within,
} from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, beforeEach, expect, it, vi } from 'vitest'
import { routes } from '../../routes'
import type { Listing } from './useListings'

vi.mock('../auth/keycloak', () => ({
  keycloak: {
    authenticated: true,
    token: 'test-token',
    updateToken: vi.fn().mockResolvedValue(false),
    login: vi.fn(),
    register: vi.fn(),
  },
  keycloakReady: Promise.resolve(),
}))

let listings: Listing[]
let activeOrders: Set<number>
let calls: { method: string; path: string; body?: string }[]

const logo: Listing = {
  id: 1,
  title: 'Logo design',
  description: 'Clean brand identities.',
  category: 'Graphic Design',
  minimumPrice: 50,
  maximumPrice: 200,
  status: 'Published',
}

// A small stand-in for the listing endpoints, with the same delete and unpublish rules.
function stubApi() {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, init: RequestInit = {}) => {
      const path = new URL(url).pathname
      const method = init.method ?? 'GET'
      calls.push({ method, path, body: init.body as string | undefined })

      if (path === '/health') return Response.json({ status: 'Healthy' })
      if (path === '/me')
        return Response.json({
          id: 'seller-1',
          username: 'sam',
          roles: ['Seller'],
        })
      if (path === '/categories') {
        return Response.json([
          { id: 3, name: 'Graphic Design' },
          { id: 4, name: 'Consulting' },
        ])
      }
      if (path === '/me/listings') return Response.json(listings)

      const match = path.match(
        /^\/me\/listings\/(\d+)(?:\/(publish|unpublish))?$/,
      )
      const listing = match && listings.find((l) => l.id === Number(match[1]))
      if (!listing) return new Response(null, { status: 404 })

      if (match[2] === 'unpublish') listing.status = 'Unpublished'
      if (match[2] === 'publish') listing.status = 'Published'
      if (method === 'PUT')
        Object.assign(listing, JSON.parse(String(init.body)))
      if (method === 'DELETE') {
        if (activeOrders.has(listing.id)) {
          return Response.json(
            { detail: 'Unpublish it instead.' },
            { status: 409 },
          )
        }
        listings = listings.filter((l) => l !== listing)
        return new Response(null, { status: 204 })
      }
      return Response.json(listing)
    }),
  )
}

function renderAt(path: string) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider
        router={createMemoryRouter(routes, { initialEntries: [path] })}
      />
    </QueryClientProvider>,
  )
}

beforeEach(() => {
  listings = [{ ...logo }]
  activeOrders = new Set()
  calls = []
  stubApi()
})

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

it('lists my services with their status', async () => {
  renderAt('/seller/listings')

  const row = (
    await screen.findByRole('heading', { name: 'Logo design' })
  ).closest('li')!
  within(row).getByText('Live')
  within(row).getByText('Graphic Design · 50–200')
})

it('unpublishes a listing and offers to republish it', async () => {
  renderAt('/seller/listings')

  fireEvent.click(await screen.findByRole('button', { name: 'Unpublish' }))

  await screen.findByText('Hidden from search')
  await screen.findByRole('button', { name: 'Republish' })
  expect(calls).toContainEqual(
    expect.objectContaining({
      method: 'POST',
      path: '/me/listings/1/unpublish',
    }),
  )
})

it('deletes a listing without active orders after asking first', async () => {
  renderAt('/seller/listings')

  fireEvent.click(await screen.findByRole('button', { name: 'Delete' }))
  const dialog = await screen.findByRole('dialog', {
    name: 'Delete "Logo design"?',
  })
  fireEvent.click(within(dialog).getByRole('button', { name: 'Delete' }))

  await screen.findByText(
    'You have no services yet. Create one to start getting requests.',
  )
})

it('asks me to unpublish instead when the listing has active orders', async () => {
  activeOrders.add(1)
  renderAt('/seller/listings')

  fireEvent.click(await screen.findByRole('button', { name: 'Delete' }))
  fireEvent.click(
    within(await screen.findByRole('dialog')).getByRole('button', {
      name: 'Delete',
    }),
  )

  const dialog = await screen.findByRole('dialog', {
    name: "This listing can't be deleted",
  })
  fireEvent.click(within(dialog).getByRole('button', { name: 'Unpublish' }))

  await screen.findByText('Hidden from search')
  expect(listings).toHaveLength(1)
})

it('saves an edit and shows it straight away', async () => {
  renderAt('/seller/listings/1/edit')

  const title = await screen.findByRole('textbox', { name: 'Title' })
  fireEvent.change(title, { target: { value: 'Logo and brand guide' } })
  fireEvent.click(screen.getByRole('button', { name: 'Save changes' }))

  await screen.findByText('Changes saved.')
  await screen.findByRole('heading', { name: 'Logo and brand guide' })
  const put = calls.find((call) => call.method === 'PUT')
  expect(JSON.parse(put!.body!)).toEqual({
    title: 'Logo and brand guide',
    description: 'Clean brand identities.',
    category: 'Graphic Design',
    minimumPrice: 50,
    maximumPrice: 200,
  })
})

it('shows field errors without saving when an edit is invalid', async () => {
  renderAt('/seller/listings/1/edit')

  fireEvent.change(
    await screen.findByRole('spinbutton', { name: 'Minimum price' }),
    {
      target: { value: '500' },
    },
  )
  fireEvent.click(screen.getByRole('button', { name: 'Save changes' }))

  await screen.findByText("Minimum price can't exceed maximum price.")
  expect(calls.some((call) => call.method === 'PUT')).toBe(false)
})

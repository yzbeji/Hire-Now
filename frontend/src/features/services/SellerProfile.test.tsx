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
import type { SellerProfile, Service } from './useServices'

// Public pages: the visitor is signed out.
vi.mock('../auth/keycloak', () => ({
  keycloak: {
    authenticated: false,
    updateToken: vi.fn().mockResolvedValue(false),
    login: vi.fn(),
    register: vi.fn(),
  },
  keycloakReady: Promise.resolve(),
}))

const logo = {
  id: 1,
  title: 'Logo and brand identity',
  category: 'Graphic Design',
  minimumPrice: 150,
  maximumPrice: 600,
  coverImageUrl: null,
  averageRating: null,
  reviewCount: 0,
}

let sellers: Record<string, SellerProfile>
let services: Record<number, Service>

function stubApi() {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string) => {
      const path = decodeURIComponent(new URL(url).pathname)
      if (path === '/health') return Response.json({ status: 'Healthy' })

      const seller = path.match(/^\/sellers\/(.+)$/)
      if (seller) {
        const found = sellers[seller[1]]
        return found
          ? Response.json(found)
          : new Response(null, { status: 404 })
      }

      const service = path.match(/^\/services\/(\d+)$/)
      if (service) {
        const found = services[Number(service[1])]
        return found
          ? Response.json(found)
          : new Response(null, { status: 404 })
      }

      return new Response(null, { status: 404 })
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
  sellers = {
    'seller-1': {
      userId: 'seller-1',
      displayName: 'Sam Seller',
      bio: 'Freelance designer.',
      location: 'Cluj-Napoca',
      profilePhoto: '',
      averageRating: null,
      reviewCount: 0,
      completedOrders: 0,
      services: [
        logo,
        {
          ...logo,
          id: 2,
          title: 'One-hour website review',
          category: 'Consulting',
          averageRating: 4.75,
          reviewCount: 12,
        },
      ],
    },
  }
  services = {
    1: {
      ...logo,
      description: 'A logo, colour palette and type pairing.',
      seller: { userId: 'seller-1', displayName: 'Sam Seller' },
    },
  }
  stubApi()
})

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

it('shows a seller profile with rating, completed orders and service cards', async () => {
  renderAt('/sellers/seller-1')

  await screen.findByRole('heading', { level: 1, name: 'Sam Seller' })
  screen.getByText('Cluj-Napoca')
  screen.getByText('Completed orders')

  const cards = within(screen.getByRole('list')).getAllByRole('listitem')
  expect(cards).toHaveLength(2)
  within(cards[0]).getByRole('heading', { name: 'Logo and brand identity' })
  within(cards[0]).getByText('150–600')
  within(cards[0]).getByText('No reviews yet')
  within(cards[1]).getByText('(12 reviews)')
  expect(within(cards[1]).getByRole('link').getAttribute('href')).toBe(
    '/services/2',
  )
})

it('says so when a seller has no services on offer', async () => {
  sellers['seller-1'].services = []

  renderAt('/sellers/seller-1')

  await screen.findByText('Sam Seller has no services on offer right now.')
})

it('explains when the seller does not exist', async () => {
  renderAt('/sellers/nobody')

  await screen.findByText("This service provider couldn't be found.")
})

it('goes from a card to the service page and back to the seller', async () => {
  renderAt('/sellers/seller-1')

  fireEvent.click(
    await screen.findByRole('link', { name: /Logo and brand identity/ }),
  )

  await screen.findByRole('heading', {
    level: 1,
    name: 'Logo and brand identity',
  })
  screen.getByText('A logo, colour palette and type pairing.')
  fireEvent.click(screen.getByRole('link', { name: 'Sam Seller' }))

  await screen.findByRole('heading', { level: 1, name: 'Sam Seller' })
})

it('explains when a service is not available', async () => {
  renderAt('/services/99')

  await screen.findByText(
    "This service isn't available. Its provider may have unpublished or removed it.",
  )
})

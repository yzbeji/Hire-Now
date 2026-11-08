import type { RouteObject } from 'react-router'
import { AppLayout } from './components/AppLayout'
import { RequireRole } from './features/auth/RequireRole'
import { AdminPage } from './pages/AdminPage'
import { BuyerPage } from './pages/BuyerPage'
import { HomePage } from './pages/HomePage'
import { NewListingPage } from './pages/NewListingPage'
import { SellerPage } from './pages/SellerPage'
import { ProfilePage } from './pages/ProfilePage'

export const routes: RouteObject[] = [
  {
    element: <AppLayout />,
    children: [
      { path: '/', element: <HomePage /> },
      {
        element: <RequireRole role="Seller" />,
        children: [
          { path: '/seller', element: <SellerPage /> },
          { path: '/seller/listings/new', element: <NewListingPage /> },
        ],
      },
      {
        element: <RequireRole role="Buyer" />,
        children: [{ path: '/buyer', element: <BuyerPage /> }],
      },
      {
        element: <RequireRole role="Admin" />,
        children: [{ path: '/admin', element: <AdminPage /> }],
      },
      {
        children: [{path: '/profile/:id?', element: <ProfilePage />}]
      }
    ],
  },
]

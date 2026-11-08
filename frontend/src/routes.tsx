import type { RouteObject } from 'react-router'
import { AppLayout } from './components/AppLayout'
import { RequireRole } from './features/auth/RequireRole'
import { AdminPage } from './pages/AdminPage'
import { BuyerPage } from './pages/BuyerPage'
import { EditListingPage } from './pages/EditListingPage'
import { HomePage } from './pages/HomePage'
import { MyListingsPage } from './pages/MyListingsPage'
import { NewListingPage } from './pages/NewListingPage'
import { SellerPage } from './pages/SellerPage'
import { SellerProfilePage } from './pages/SellerProfilePage'
import { ServicePage } from './pages/ServicePage'
import { ProfilePage } from './pages/ProfilePage'

export const routes: RouteObject[] = [
  {
    element: <AppLayout />,
    children: [
      { path: '/', element: <HomePage /> },
      { path: '/sellers/:id', element: <SellerProfilePage /> },
      { path: '/services/:id', element: <ServicePage /> },
      {
        element: <RequireRole role="Seller" />,
        children: [
          { path: '/seller', element: <SellerPage /> },
          { path: '/seller/listings', element: <MyListingsPage /> },
          { path: '/seller/listings/new', element: <NewListingPage /> },
          { path: '/seller/listings/:id/edit', element: <EditListingPage /> },
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
        children: [{ path: '/profile/:id?', element: <ProfilePage /> }],
      },
    ],
  },
]

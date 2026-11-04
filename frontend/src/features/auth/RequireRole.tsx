import { CircularProgress } from '@mui/material'
import { Navigate, Outlet } from 'react-router'
import { useAuth } from './useAuth'
import { useCurrentUser, type Role } from './useCurrentUser'

export function RequireRole({ role }: { role: Role }) {
  const { isReady, isAuthenticated } = useAuth()
  const currentUser = useCurrentUser()

  if (!isReady || (isAuthenticated && currentUser.isPending)) {
    return <CircularProgress aria-label="Loading" />
  }

  return currentUser.data?.roles.includes(role) ? (
    <Outlet />
  ) : (
    <Navigate to="/" replace />
  )
}

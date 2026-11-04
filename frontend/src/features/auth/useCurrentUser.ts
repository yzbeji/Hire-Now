import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../api/client'
import { useAuth } from './useAuth'

export type Role = 'Seller' | 'Buyer' | 'Admin'

type CurrentUser = { username: string; roles: Role[] }

export function useCurrentUser() {
  const { isAuthenticated } = useAuth()

  return useQuery({
    queryKey: ['me'],
    queryFn: () => apiGet<CurrentUser>('/me'),
    enabled: isAuthenticated,
  })
}

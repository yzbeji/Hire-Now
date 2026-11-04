import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../api/client'
import { useAuth } from './useAuth'

type CurrentUser = { username: string; roles: string[] }

export function useCurrentUser() {
  const { isAuthenticated } = useAuth()

  return useQuery({
    queryKey: ['me'],
    queryFn: () => apiGet<CurrentUser>('/me'),
    enabled: isAuthenticated,
  })
}

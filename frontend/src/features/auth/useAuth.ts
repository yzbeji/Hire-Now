import { useQuery } from '@tanstack/react-query'
import { keycloak, keycloakReady } from './keycloak'

export function useAuth() {
  const { data } = useQuery({
    queryKey: ['auth'],
    queryFn: async () => {
      await keycloakReady
      return Boolean(keycloak.authenticated)
    },
    staleTime: Infinity,
  })

  return { isReady: data !== undefined, isAuthenticated: data === true }
}

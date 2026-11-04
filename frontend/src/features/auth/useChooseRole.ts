import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiPost } from '../../api/client'
import { keycloak } from './keycloak'

export function useChooseRole() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (role: 'Seller' | 'Buyer') => apiPost('/me/role', { role }),
    // Also after a failure: another tab may have chosen first, and only a new token shows the role.
    onSettled: async () => {
      await keycloak.updateToken(-1).catch(() => undefined)
      await queryClient.invalidateQueries({ queryKey: ['me'] })
    },
  })
}

import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../api/client'

export function useApiHealth() {
  return useQuery({
    queryKey: ['health'],
    queryFn: () => apiGet<{ status: string }>('/health'),
  })
}

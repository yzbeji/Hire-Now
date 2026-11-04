import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../api/client'

export function HomePage() {
  const health = useQuery({
    queryKey: ['health'],
    queryFn: () => apiGet<{ status: string }>('/health'),
  })

  return (
    <main>
      <h1>HireNow</h1>
      <p role="status">
        API:{' '}
        {health.data?.status ?? (health.isError ? 'unreachable' : 'checking…')}
      </p>
    </main>
  )
}

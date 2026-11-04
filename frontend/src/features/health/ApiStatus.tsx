import { useApiHealth } from './useApiHealth'

export function ApiStatus() {
  const health = useApiHealth()

  return (
    <p role="status">
      API:{' '}
      {health.data?.status ?? (health.isError ? 'unreachable' : 'checking…')}
    </p>
  )
}

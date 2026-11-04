import { Typography } from '@mui/material'
import { useApiHealth } from './useApiHealth'

export function ApiStatus() {
  const health = useApiHealth()

  return (
    <Typography role="status" variant="body2" color="text.secondary">
      API:{' '}
      {health.data?.status ?? (health.isError ? 'unreachable' : 'checking…')}
    </Typography>
  )
}

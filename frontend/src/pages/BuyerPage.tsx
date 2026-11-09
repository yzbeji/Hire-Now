import { Alert, Chip, CircularProgress, Link as MuiLink, Paper, Stack, Typography } from '@mui/material'
import { Link } from 'react-router'
import { AreaNotice } from '../components/AreaNotice'
import { useMyRequests } from '../features/requests/useRequests'

export function BuyerPage() {
  const requests = useMyRequests()

  return (
    <Stack spacing={3} sx={{ maxWidth: 960 }}>
      <AreaNotice title="Buyer area" tone="seeker">
        Your requests and orders will appear here.
      </AreaNotice>

      {requests.isPending && <CircularProgress aria-label="Loading your requests" />}
      {requests.isError && <Alert severity="error">Your requests couldn't be loaded.</Alert>}
      {requests.data?.length === 0 && (
        <Typography>You haven't requested any services yet.</Typography>
      )}

      <Stack component="ul" spacing={2} sx={{ listStyle: 'none', p: 0, m: 0 }}>
        {requests.data?.map((r) => (
          <Paper key={r.id} component="li" variant="outlined" sx={{ p: 2 }}>
            <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
              <Typography variant="h6" component="h2">
                <MuiLink component={Link} to={`/services/${r.serviceId}`}>
                  {r.serviceTitle}
                </MuiLink>
              </Typography>
              <Chip size="small" label={r.status} variant="outlined" />
            </Stack>
            <Typography color="text.secondary">
              Sent {new Date(r.createdAt).toLocaleDateString()}
              {r.budget != null && ` · Budget ${r.budget}`}
            </Typography>
            <Typography sx={{ mt: 1, overflowWrap: 'anywhere' }}>{r.message}</Typography>
          </Paper>
        ))}
      </Stack>
    </Stack>
  )
}
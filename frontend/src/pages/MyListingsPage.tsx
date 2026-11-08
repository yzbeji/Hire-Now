import { Alert, Button, Stack, Typography } from '@mui/material'
import { Link, useLocation } from 'react-router'
import { useCurrentUser } from '../features/auth/useCurrentUser'
import { MyListings } from '../features/listings/MyListings'

export function MyListingsPage() {
  const { data: me } = useCurrentUser()
  const notice = (useLocation().state as { notice?: string } | null)?.notice

  return (
    <Stack spacing={3} sx={{ maxWidth: 960 }}>
      <Stack
        direction="row"
        spacing={2}
        sx={{
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          rowGap: 2,
        }}
      >
        <Typography variant="h2" component="h1">
          My services
        </Typography>
        <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', rowGap: 2 }}>
          {me && (
            <Button
              component={Link}
              to={`/sellers/${encodeURIComponent(me.id)}`}
              variant="outlined"
            >
              View my public profile
            </Button>
          )}
          <Button
            component={Link}
            to="/seller/listings/new"
            variant="contained"
          >
            Create a listing
          </Button>
        </Stack>
      </Stack>
      {notice && <Alert severity="success">{notice}</Alert>}
      <MyListings />
    </Stack>
  )
}

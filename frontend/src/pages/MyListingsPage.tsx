import { Alert, Button, Stack, Typography } from '@mui/material'
import { Link, useLocation } from 'react-router'
import { MyListings } from '../features/listings/MyListings'

export function MyListingsPage() {
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
        <Button component={Link} to="/seller/listings/new" variant="contained">
          Create a listing
        </Button>
      </Stack>
      {notice && <Alert severity="success">{notice}</Alert>}
      <MyListings />
    </Stack>
  )
}

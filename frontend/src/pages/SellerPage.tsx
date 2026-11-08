import { Button, Stack, Typography } from '@mui/material'
import { Link } from 'react-router'
import { AreaNotice } from '../components/AreaNotice'

export function SellerPage() {
  return (
    <AreaNotice title="Seller area" tone="provider">
      <Stack spacing={2}>
        <Typography variant="body1">
          Your listings and orders will appear here.
        </Typography>
        <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', rowGap: 2 }}>
          <Button component={Link} to="/seller/listings" variant="contained">
            My services
          </Button>
          <Button component={Link} to="/seller/listings/new" variant="outlined">
            Create a listing
          </Button>
        </Stack>
      </Stack>
    </AreaNotice>
  )
}

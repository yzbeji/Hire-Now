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
        <Button component={Link} to="/seller/listings/new" variant="contained">
          Create a listing
        </Button>
      </Stack>
    </AreaNotice>
  )
}

import { Button, Paper, Stack, Typography } from '@mui/material'
import { Link } from 'react-router'
import { CreateListing } from '../features/listings/CreateListing'

export function NewListingPage() {
  return (
    <Paper variant="outlined" sx={{ maxWidth: 860, p: { xs: 2, sm: 4 } }}>
      <Stack spacing={3}>
        <Stack
          direction="row"
          spacing={2}
          sx={{ justifyContent: 'space-between', flexWrap: 'wrap', rowGap: 2 }}
        >
          <Typography variant="h3" component="h1">
            Create a listing
          </Typography>
          <Button component={Link} to="/seller/listings" variant="outlined">
            Back to my services
          </Button>
        </Stack>
        <CreateListing />
      </Stack>
    </Paper>
  )
}

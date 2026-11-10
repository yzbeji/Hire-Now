import { Paper, Stack, Typography } from '@mui/material'
import { useParams } from 'react-router'
import { EditListing } from '../features/listings/EditListing'

export function EditListingPage() {
  const { id } = useParams()

  return (
    <Paper variant="outlined" sx={{ maxWidth: 860, p: { xs: 2, sm: 4 } }}>
      <Stack spacing={3}>
        <Typography variant="h3" component="h1">
          Edit listing
        </Typography>
        <EditListing key={id} id={Number(id)} />
      </Stack>
    </Paper>
  )
}

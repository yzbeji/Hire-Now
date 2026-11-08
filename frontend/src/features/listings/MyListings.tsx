import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { Link } from 'react-router'
import {
  hasActiveOrders,
  useDeleteListing,
  useMyListings,
  usePublishListing,
  useUnpublishListing,
  type Listing,
  type ListingStatus,
} from './useListings'

const statusLabels: Record<ListingStatus, string> = {
  Published: 'Live',
  Draft: 'Draft',
  Unpublished: 'Hidden from search',
}

export function MyListings() {
  const listings = useMyListings()
  const [deleting, setDeleting] = useState<Listing | null>(null)

  if (listings.isPending)
    return <CircularProgress aria-label="Loading your services" />
  if (listings.isError) {
    return (
      <Alert severity="error">
        Your services couldn't be loaded. Refresh the page to try again.
      </Alert>
    )
  }

  if (listings.data.length === 0) {
    return (
      <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
        <Typography>
          You have no services yet. Create one to start getting requests.
        </Typography>
        <Button component={Link} to="/seller/listings/new" variant="contained">
          Create a listing
        </Button>
      </Stack>
    )
  }

  return (
    <>
      <Stack component="ul" spacing={2} sx={{ listStyle: 'none', p: 0, m: 0 }}>
        {listings.data.map((listing) => (
          <ListingRow
            key={listing.id}
            listing={listing}
            onDelete={() => setDeleting(listing)}
          />
        ))}
      </Stack>
      {deleting && (
        <DeleteListingDialog
          listing={deleting}
          onClose={() => setDeleting(null)}
        />
      )}
    </>
  )
}

function ListingRow({
  listing,
  onDelete,
}: {
  listing: Listing
  onDelete: () => void
}) {
  const publish = usePublishListing()
  const unpublish = useUnpublishListing()
  const busy = publish.isPending || unpublish.isPending
  const failed = publish.isError || unpublish.isError

  return (
    <Paper component="li" variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
      <Stack
        direction={{ xs: 'column', md: 'row' }}
        spacing={2}
        sx={{ justifyContent: 'space-between', alignItems: { md: 'center' } }}
      >
        <Box sx={{ minWidth: 0 }}>
          <Stack
            direction="row"
            spacing={1.5}
            sx={{ alignItems: 'center', flexWrap: 'wrap', rowGap: 1 }}
          >
            <Typography
              variant="h6"
              component="h2"
              sx={{ overflowWrap: 'anywhere' }}
            >
              {listing.title}
            </Typography>
            <Chip
              size="small"
              label={statusLabels[listing.status]}
              color={listing.status === 'Published' ? 'primary' : 'default'}
              variant={listing.status === 'Published' ? 'filled' : 'outlined'}
            />
          </Stack>
          <Typography color="text.secondary">
            {listing.category} · {listing.minimumPrice}–{listing.maximumPrice}
          </Typography>
          {failed && (
            <Typography color="error" role="alert">
              That didn't work. Try again.
            </Typography>
          )}
        </Box>

        <Stack
          direction="row"
          spacing={1}
          sx={{ flexWrap: 'wrap', rowGap: 1, flexShrink: 0 }}
        >
          <Button
            component={Link}
            to={`/seller/listings/${listing.id}/edit`}
            variant="outlined"
          >
            Edit
          </Button>
          {listing.status === 'Published' ? (
            <Button
              variant="outlined"
              disabled={busy}
              onClick={() => unpublish.mutate(listing.id)}
            >
              Unpublish
            </Button>
          ) : (
            <Button
              variant="contained"
              disabled={busy}
              onClick={() => publish.mutate(listing.id)}
            >
              {listing.status === 'Draft' ? 'Publish' : 'Republish'}
            </Button>
          )}
          <Button color="error" onClick={onDelete}>
            Delete
          </Button>
        </Stack>
      </Stack>
    </Paper>
  )
}

function DeleteListingDialog({
  listing,
  onClose,
}: {
  listing: Listing
  onClose: () => void
}) {
  const remove = useDeleteListing()
  const unpublish = useUnpublishListing()
  const blocked = hasActiveOrders(remove.error)

  return (
    <Dialog open onClose={onClose} aria-labelledby="delete-listing-title">
      <DialogTitle id="delete-listing-title">
        {blocked
          ? "This listing can't be deleted"
          : `Delete "${listing.title}"?`}
      </DialogTitle>
      <DialogContent>
        <DialogContentText>
          {blocked
            ? listing.status === 'Published'
              ? 'It has active orders. Unpublish it instead: it disappears from search, and the orders carry on.'
              : 'It has active orders. It is already hidden from search, and the orders carry on.'
            : "This removes it for good. You can't undo it."}
        </DialogContentText>
        {remove.isError && !blocked && (
          <Alert severity="error" sx={{ mt: 2 }}>
            The listing wasn't deleted. Try again.
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{blocked ? 'Close' : 'Keep it'}</Button>
        {blocked ? (
          listing.status === 'Published' && (
            <Button
              variant="contained"
              disabled={unpublish.isPending}
              onClick={() =>
                unpublish.mutate(listing.id, { onSuccess: onClose })
              }
            >
              Unpublish
            </Button>
          )
        ) : (
          <Button
            variant="contained"
            color="error"
            disabled={remove.isPending}
            onClick={() => remove.mutate(listing.id, { onSuccess: onClose })}
          >
            Delete
          </Button>
        )}
      </DialogActions>
    </Dialog>
  )
}

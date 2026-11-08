import { Alert, Button, CircularProgress } from '@mui/material'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { ListingForm } from './ListingForm'
import {
  toFormValues,
  toListingFields,
  validateListing,
  type ListingFormErrors,
  type ListingFormValues,
} from './listingFormValues'
import {
  fieldErrorsFrom,
  useMyListing,
  useUpdateListing,
  type Listing,
} from './useListings'

export function EditListing({ id }: { id: number }) {
  const listing = useMyListing(id)

  if (listing.isPending)
    return <CircularProgress aria-label="Loading your listing" />
  if (listing.isError) {
    return (
      <Alert severity="error">
        This listing couldn't be opened. It may have been deleted, or it belongs
        to another service provider.
      </Alert>
    )
  }

  return <EditListingForm listing={listing.data} />
}

function EditListingForm({ listing }: { listing: Listing }) {
  const navigate = useNavigate()
  const update = useUpdateListing(listing.id)
  const [values, setValues] = useState<ListingFormValues>(() =>
    toFormValues(listing),
  )
  const [errors, setErrors] = useState<ListingFormErrors>({})

  function save() {
    const nextErrors = validateListing(values)
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length > 0) return

    update.mutate(toListingFields(values), {
      onSuccess: () =>
        navigate('/seller/listings', { state: { notice: 'Changes saved.' } }),
      onError: (error) => setErrors(fieldErrorsFrom(error)),
    })
  }

  return (
    <>
      {update.isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Your changes weren't saved. Fix the highlighted fields and try again.
        </Alert>
      )}
      <ListingForm
        values={values}
        errors={errors}
        onChange={(next, changed) => {
          setValues(next)
          setErrors((current) => ({ ...current, [changed]: undefined }))
        }}
        actions={
          <>
            <Button
              variant="contained"
              onClick={save}
              disabled={update.isPending}
            >
              Save changes
            </Button>
            <Button component={Link} to="/seller/listings">
              Cancel
            </Button>
          </>
        }
      />
    </>
  )
}

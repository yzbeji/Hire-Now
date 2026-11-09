import { Alert, Button } from '@mui/material'
import { useState } from 'react'
import { useNavigate } from 'react-router'
import { ListingForm } from './ListingForm'
import {
  emptyListingForm,
  toListingFields,
  validateListing,
  type ListingFormErrors,
} from './listingFormValues'
import { fieldErrorsFrom, useCreateListing } from './useListings'

export function CreateListing() {
  const navigate = useNavigate()
  const create = useCreateListing()
  const [values, setValues] = useState(emptyListingForm)
  const [errors, setErrors] = useState<ListingFormErrors>({})

  function submit(status: 'Draft' | 'Published') {
    const nextErrors = validateListing(values)
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length > 0) return

    create.mutate(
      { ...toListingFields(values), status },
      {
        onSuccess: () =>
          navigate('/seller/listings', {
            state: {
              notice:
                status === 'Published'
                  ? 'Published. Your listing is visible to service seekers.'
                  : 'Draft saved. Only you can see it.',
            },
          }),
        onError: (error) => setErrors(fieldErrorsFrom(error)),
      },
    )
  }

  return (
    <>
      {create.isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Your listing wasn't saved. Fix the highlighted fields and try again.
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
              variant="outlined"
              onClick={() => submit('Draft')}
              disabled={create.isPending}
            >
              Save draft
            </Button>
            <Button
              variant="contained"
              onClick={() => submit('Published')}
              disabled={create.isPending}
            >
              Publish
            </Button>
          </>
        }
      />
    </>
  )
}

import type { ListingFields } from './useListings'

export type ListingFormValues = {
  title: string
  description: string
  category: string
  minimumPrice: string
  maximumPrice: string
}

export type ListingFormErrors = Partial<Record<keyof ListingFormValues, string>>

export const emptyListingForm: ListingFormValues = {
  title: '',
  description: '',
  category: '',
  minimumPrice: '',
  maximumPrice: '',
}

export function toFormValues(listing: ListingFields): ListingFormValues {
  return {
    title: listing.title,
    description: listing.description,
    category: listing.category,
    minimumPrice: String(listing.minimumPrice),
    maximumPrice: String(listing.maximumPrice),
  }
}

export function toListingFields(form: ListingFormValues): ListingFields {
  return {
    title: form.title.trim(),
    description: form.description.trim(),
    category: form.category,
    minimumPrice: Number(form.minimumPrice),
    maximumPrice: Number(form.maximumPrice),
  }
}

export function validateListing(form: ListingFormValues): ListingFormErrors {
  const errors: ListingFormErrors = {}
  const title = form.title.trim()
  if (!title) {
    errors.title = 'Title is required.'
  } else if (title.length > 100) {
    errors.title = 'Title must be 100 characters or fewer.'
  }

  const description = form.description.trim()
  if (!description) {
    errors.description = 'Description is required.'
  } else if (description.length > 5000) {
    errors.description = 'Description must be 5000 characters or fewer.'
  }

  if (!form.category) {
    errors.category = 'Category is required.'
  }

  const minimumPrice = Number(form.minimumPrice)
  const maximumPrice = Number(form.maximumPrice)

  if (form.minimumPrice.trim() === '') {
    errors.minimumPrice = 'Minimum price is required.'
  } else if (Number.isNaN(minimumPrice) || minimumPrice <= 0) {
    errors.minimumPrice = 'Minimum price must be greater than 0.'
  }

  if (form.maximumPrice.trim() === '') {
    errors.maximumPrice = 'Maximum price is required.'
  } else if (Number.isNaN(maximumPrice) || maximumPrice <= 0) {
    errors.maximumPrice = 'Maximum price must be greater than 0.'
  }

  if (
    !errors.minimumPrice &&
    !errors.maximumPrice &&
    minimumPrice > maximumPrice
  ) {
    errors.minimumPrice = "Minimum price can't exceed maximum price."
  }

  return errors
}

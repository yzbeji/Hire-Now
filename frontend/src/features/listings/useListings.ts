import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiDelete, apiGet, apiPost, apiPut } from '../../api/client'

export type ListingStatus = 'Draft' | 'Published' | 'Unpublished'

export type Listing = {
  id: number
  title: string
  description: string
  category: string
  minimumPrice: number
  maximumPrice: number
  status: ListingStatus
}

export type ListingFields = Omit<Listing, 'id' | 'status'>

type ApiError = Error & { status?: number; payload?: unknown }

const myListingsKey = ['me', 'listings'] as const

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: () => apiGet<{ id: number; name: string }[]>('/categories'),
    staleTime: Infinity,
  })
}

export function useMyListings() {
  return useQuery({
    queryKey: myListingsKey,
    queryFn: () => apiGet<Listing[]>('/me/listings'),
  })
}

export function useMyListing(id: number) {
  return useQuery({
    queryKey: [...myListingsKey, id],
    queryFn: () => apiGet<Listing>(`/me/listings/${id}`),
  })
}

function useListingMutation<TVariables>(
  mutationFn: (variables: TVariables) => Promise<unknown>,
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: myListingsKey }),
  })
}

export function useCreateListing() {
  return useListingMutation(
    (fields: ListingFields & { status: 'Draft' | 'Published' }) =>
      apiPost<Listing>('/me/listings', fields),
  )
}

export function useUpdateListing(id: number) {
  return useListingMutation((fields: ListingFields) =>
    apiPut<Listing>(`/me/listings/${id}`, fields),
  )
}

export function usePublishListing() {
  return useListingMutation((id: number) =>
    apiPost<Listing>(`/me/listings/${id}/publish`),
  )
}

export function useUnpublishListing() {
  return useListingMutation((id: number) =>
    apiPost<Listing>(`/me/listings/${id}/unpublish`),
  )
}

export function useDeleteListing() {
  return useListingMutation((id: number) => apiDelete(`/me/listings/${id}`))
}

/** The API answers 409 when a listing with active orders is deleted. */
export function hasActiveOrders(error: unknown) {
  return (error as ApiError | null)?.status === 409
}

/** Turns an ASP.NET validation problem into one message per form field. */
export function fieldErrorsFrom(
  error: unknown,
): Partial<Record<keyof ListingFields, string>> {
  const payload = (error as ApiError | null)?.payload as
    { errors?: Record<string, string[]> } | undefined
  return Object.fromEntries(
    Object.entries(payload?.errors ?? {}).map(([key, messages]) => [
      key,
      messages[0],
    ]),
  )
}

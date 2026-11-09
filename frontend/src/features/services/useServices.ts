import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../api/client'

/** A service as shown on a card. Search (HN-13) returns the same shape. */
export type ServiceSummary = {
  id: number
  title: string
  category: string
  minimumPrice: number
  maximumPrice: number
  coverImageUrl: string | null
  averageRating: number | null
  reviewCount: number
}

export type SellerProfile = {
  userId: string
  displayName: string
  bio: string
  location: string
  profilePhoto: string
  averageRating: number | null
  reviewCount: number
  completedOrders: number
  services: ServiceSummary[]
}

export type Service = ServiceSummary & {
  description: string
  seller: { userId: string; displayName: string }
}

export function useSellerProfile(userId: string) {
  return useQuery({
    queryKey: ['sellers', userId],
    queryFn: () =>
      apiGet<SellerProfile>(`/sellers/${encodeURIComponent(userId)}`),
  })
}

export function useService(id: number) {
  return useQuery({
    queryKey: ['services', id],
    queryFn: () => apiGet<Service>(`/services/${id}`),
  })
}

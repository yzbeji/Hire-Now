import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiGet, apiPost } from '../../api/client'

export type RequestStatus = 'Pending' | 'Accepted' | 'Declined' | 'Cancelled'

export type ServiceRequest = {
    id: number
    serviceId: number
    serviceTitle: string
    message: string
    budget: number | null
    preferredDate: string | null
    status: RequestStatus
    createdAt: string
}

export type NewRequest = {
    message: string
    budget: number | null
    preferredDate: string | null
}

export function useMyRequests() {
    return useQuery<ServiceRequest[]>({
    queryKey: ['requests', 'mine'],
    queryFn: () => apiGet<ServiceRequest[]>('/me/requests'), 
    })
}

export function useCreateRequest(serviceId: number) {
    const queryClient = useQueryClient()
    return useMutation({
    mutationFn: (values: NewRequest) =>
    apiPost<ServiceRequest>(`/services/${serviceId}/requests`, values), 
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['requests'] }),
    })
}
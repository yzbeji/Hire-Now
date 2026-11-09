import { Alert, Button, Stack } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { apiPost } from '../../api/client'
import { useCurrentUser } from '../auth/useCurrentUser'

export function ChatButton({ sellerId }: { sellerId: string }) {
  const { data: user } = useCurrentUser()
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const start = useMutation({
    mutationFn: () => apiPost<{ id: string }>('/conversations', { sellerId }),
    onSuccess: (conversation) => {
      void queryClient.invalidateQueries({
        queryKey: ['chat', user?.id, 'conversations'],
      })
      void navigate(`/chat/${conversation.id}`)
    },
  })

  if (!user?.roles.includes('Buyer') || user.id === sellerId) return null

  return (
    <Stack spacing={1}>
      <Button
        variant="contained"
        onClick={() => start.mutate()}
        disabled={start.isPending}
      >
        {start.isPending ? 'Opening chat...' : 'Contact seller'}
      </Button>
      {start.isError && (
        <Alert severity="error">Could not open chat. Please try again.</Alert>
      )}
    </Stack>
  )
}

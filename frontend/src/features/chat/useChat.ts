import {
  HubConnectionBuilder,
  HubConnectionState,
  type HubConnection,
} from '@microsoft/signalr'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { apiGet, baseUrl, getToken } from '../../api/client'

export type Conversation = {
  id: string
  buyerId: string
  sellerId: string
  participantName: string
}

export type ChatMessage = {
  id: string
  conversationId: string
  senderId: string
  content: string
  sentAt: string
}

function mergeMessages(history: ChatMessage[], live: ChatMessage[]) {
  return [
    ...new Map([...history, ...live].map((m) => [m.id, m])).values(),
  ].sort((a, b) => a.sentAt.localeCompare(b.sentAt) || a.id.localeCompare(b.id))
}

export function useChat(userId: string, requestedId?: string) {
  const queryClient = useQueryClient()
  const connectionRef = useRef<HubConnection>(null)
  const [isConnected, setIsConnected] = useState(false)
  const [connectionError, setConnectionError] = useState<string | null>(null)
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    let disposed = false
    const connection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/chat`, {
        accessTokenFactory: async () => (await getToken()) ?? '',
      })
      .withAutomaticReconnect()
      .build()
    connectionRef.current = connection

    const refresh = () => {
      if (disposed) return
      setIsConnected(true)
      setConnectionError(null)
      // Reload history after reconnecting to include messages sent while offline.
      void queryClient.invalidateQueries({ queryKey: ['chat', userId] })
    }

    connection.on('ReceiveMessage', (message: ChatMessage) => {
      if (disposed) return
      queryClient.setQueryData<ChatMessage[]>(
        ['chat', userId, 'messages', message.conversationId],
        (previous = []) => mergeMessages(previous, [message]),
      )
      // A buyer's first message also makes a new conversation appear for the seller.
      void queryClient.invalidateQueries({
        queryKey: ['chat', userId, 'conversations'],
      })
    })
    connection.onreconnecting(() => {
      if (!disposed) setIsConnected(false)
    })
    connection.onreconnected(refresh)
    connection.onclose(() => {
      if (disposed) return
      setIsConnected(false)
      setConnectionError('Chat disconnected. Reconnect to send messages.')
    })
    void connection
      .start()
      .then(refresh)
      .catch(() => {
        if (disposed) return
        setIsConnected(false)
        setConnectionError('Could not connect to chat. Please try again.')
      })

    return () => {
      disposed = true
      connectionRef.current = null
      void connection.stop()
    }
  }, [userId, queryClient, attempt])

  const conversations = useQuery({
    queryKey: ['chat', userId, 'conversations'],
    queryFn: () => apiGet<Conversation[]>('/conversations'),
  })
  const activeId = requestedId ?? conversations.data?.[0]?.id
  const messages = useQuery({
    queryKey: ['chat', userId, 'messages', activeId],
    queryFn: async () => {
      const history = await apiGet<ChatMessage[]>(
        `/conversations/${activeId}/messages`,
      )
      // Preserve live messages that arrived while the history request was in flight.
      return mergeMessages(
        history,
        queryClient.getQueryData<ChatMessage[]>([
          'chat',
          userId,
          'messages',
          activeId,
        ]) ?? [],
      )
    },
    enabled: !!activeId,
    retry: false,
  })

  async function sendMessage(content: string) {
    const connection = connectionRef.current
    if (!activeId || connection?.state !== HubConnectionState.Connected) {
      throw new Error('Chat is disconnected. Please reconnect and try again.')
    }
    const message = await connection.invoke<ChatMessage>(
      'SendMessage',
      activeId,
      content,
    )
    queryClient.setQueryData<ChatMessage[]>(
      ['chat', userId, 'messages', message.conversationId],
      (previous = []) => mergeMessages(previous, [message]),
    )
  }

  return {
    conversations,
    messages,
    activeId,
    activeConversation: conversations.data?.find((c) => c.id === activeId),
    isConnected,
    connectionError,
    reconnect: () => {
      setConnectionError(null)
      setAttempt((value) => value + 1)
    },
    sendMessage,
  }
}

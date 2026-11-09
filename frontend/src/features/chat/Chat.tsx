import {
  Alert,
  Box,
  Button,
  CircularProgress,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router'
import { keycloak } from '../auth/keycloak'
import { useAuth } from '../auth/useAuth'
import { useCurrentUser } from '../auth/useCurrentUser'
import { useChat, type ChatMessage } from './useChat'

export function Chat() {
  const { isReady, isAuthenticated } = useAuth()
  const currentUser = useCurrentUser()

  if (!isReady || (isAuthenticated && currentUser.isPending)) {
    return <CircularProgress aria-label="Loading chat" />
  }
  if (!isAuthenticated) {
    return (
      <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
        <Typography>Sign in to open your conversations.</Typography>
        <Button variant="contained" onClick={() => keycloak.login()}>
          Sign in
        </Button>
      </Stack>
    )
  }
  if (!currentUser.data) {
    return <Alert severity="error">Could not load your account.</Alert>
  }
  return <ChatView key={currentUser.data.id} userId={currentUser.data.id} />
}

function ChatView({ userId }: { userId: string }) {
  const { conversationId } = useParams()
  const chat = useChat(userId, conversationId)

  return (
    <Stack spacing={2}>
      <Typography variant="h2" component="h1">
        Chat
      </Typography>
      {chat.connectionError ? (
        <Alert
          severity="error"
          action={
            <Button color="inherit" onClick={chat.reconnect}>
              Reconnect
            </Button>
          }
        >
          {chat.connectionError}
        </Alert>
      ) : (
        !chat.isConnected && (
          <Alert severity="info">Connecting to chat...</Alert>
        )
      )}
      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        <Paper sx={{ width: { xs: '100%', md: 280 }, flexShrink: 0, p: 2 }}>
          <Typography variant="h3" component="h2">
            Conversations
          </Typography>
          {chat.conversations.isPending && (
            <CircularProgress aria-label="Loading conversations" />
          )}
          {chat.conversations.isError && (
            <Alert severity="error">Could not load conversations.</Alert>
          )}
          {chat.conversations.data?.length === 0 && (
            <Typography sx={{ mt: 2 }}>No conversations yet.</Typography>
          )}
          <List aria-label="Conversations">
            {chat.conversations.data?.map((conversation) => (
              <ListItemButton
                key={conversation.id}
                component={Link}
                to={`/chat/${conversation.id}`}
                selected={conversation.id === chat.activeId}
              >
                <ListItemText
                  primary={conversation.participantName}
                  secondary={
                    conversation.buyerId === userId
                      ? 'Service provider'
                      : 'Service seeker'
                  }
                />
              </ListItemButton>
            ))}
          </List>
        </Paper>
        <Paper sx={{ flex: 1, minWidth: 0, p: 2 }}>
          {chat.activeId ? (
            <MessageThread
              key={chat.activeId}
              userId={userId}
              participantName={
                chat.activeConversation?.participantName ?? 'Conversation'
              }
              messages={chat.messages.data ?? []}
              isLoading={chat.messages.isPending}
              isError={chat.messages.isError}
              canSend={
                chat.isConnected &&
                !!chat.activeConversation &&
                chat.messages.isSuccess
              }
              sendMessage={chat.sendMessage}
            />
          ) : (
            <Typography>Select a conversation to start chatting.</Typography>
          )}
        </Paper>
      </Stack>
    </Stack>
  )
}

function MessageThread({
  userId,
  participantName,
  messages,
  isLoading,
  isError,
  canSend,
  sendMessage,
}: {
  userId: string
  participantName: string
  messages: ChatMessage[]
  isLoading: boolean
  isError: boolean
  canSend: boolean
  sendMessage: (content: string) => Promise<void>
}) {
  const [content, setContent] = useState('')
  const [isSending, setIsSending] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const end = useRef<HTMLDivElement>(null)

  useEffect(() => {
    end.current?.scrollIntoView({ block: 'nearest' })
  }, [messages])

  async function send() {
    if (!content.trim() || isSending || !canSend) return
    setIsSending(true)
    setError(null)
    try {
      await sendMessage(content.trim())
      setContent('')
    } catch {
      setError('Could not send your message. Please try again.')
    } finally {
      setIsSending(false)
    }
  }

  return (
    <Stack spacing={2}>
      <Typography variant="h3" component="h2">
        {participantName}
      </Typography>
      {isLoading && <CircularProgress aria-label="Loading messages" />}
      {isError && (
        <Alert severity="error">Could not load this conversation.</Alert>
      )}
      <Box
        role="log"
        aria-label="Messages"
        aria-live="polite"
        sx={{ height: 360, overflowY: 'auto' }}
      >
        {!isLoading && !isError && messages.length === 0 && (
          <Typography>No messages yet.</Typography>
        )}
        <Stack spacing={2}>
          {messages.map((message) => (
            <Box
              key={message.id}
              sx={{
                alignSelf:
                  message.senderId === userId ? 'flex-end' : 'flex-start',
                maxWidth: '85%',
                p: 1.5,
                bgcolor: 'action.hover',
                borderRadius: 1,
              }}
            >
              <Typography variant="caption" component="p">
                {message.senderId === userId ? 'You' : participantName}
                {' · '}
                <time dateTime={message.sentAt}>
                  {new Date(message.sentAt).toLocaleString()}
                </time>
              </Typography>
              <Typography
                sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}
              >
                {message.content}
              </Typography>
            </Box>
          ))}
          <div ref={end} />
        </Stack>
      </Box>
      {error && <Alert severity="error">{error}</Alert>}
      <Stack
        component="form"
        spacing={1}
        onSubmit={(event) => {
          event.preventDefault()
          void send()
        }}
      >
        <TextField
          label="Message"
          multiline
          minRows={2}
          value={content}
          onChange={(event) => setContent(event.target.value)}
          slotProps={{ htmlInput: { maxLength: 2000 } }}
          disabled={!canSend || isSending}
          fullWidth
        />
        <Button
          type="submit"
          variant="contained"
          disabled={!canSend || isSending || !content.trim()}
        >
          {isSending ? 'Sending...' : 'Send'}
        </Button>
      </Stack>
    </Stack>
  )
}

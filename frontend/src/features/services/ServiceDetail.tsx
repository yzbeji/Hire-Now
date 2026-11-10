import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Link as MuiLink,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { Link } from 'react-router'
import { useCurrentUser } from '../auth/useCurrentUser'
import { RequestServiceDialog } from '../requests/RequestServiceDialog'
import { ChatButton } from '../chat/ChatButton'
import { formatPriceRange } from './format'
import { Rating } from './Rating'
import { useService } from './useServices'

export function ServiceDetail({ id }: { id: number }) {
  const service = useService(id)
  const { data: me } = useCurrentUser()
  const [requesting, setRequesting] = useState(false)

  if (service.isPending)
    return <CircularProgress aria-label="Loading the service" />
  if (service.isError) {
    return (
      <Alert severity="error">
        This service isn't available. Its provider may have unpublished or removed it.
      </Alert>
    )
  }

  const { data } = service
  const canRequest =
    !!me && me.roles.includes('Buyer') && data.seller.userId !== me.id

  return (
    <Paper variant="outlined" sx={{ maxWidth: 860, p: { xs: 2, sm: 4 } }}>
      <Stack spacing={3}>
        {data.coverImageUrl && (
          <Box
            component="img"
            src={data.coverImageUrl}
            alt=""
            sx={{ width: '100%', aspectRatio: '16 / 9', objectFit: 'cover' }}
          />
        )}
        <Stack spacing={1}>
          <Typography variant="body2" color="text.secondary">
            {data.category}
          </Typography>
          <Typography
            variant="h2"
            component="h1"
            sx={{ overflowWrap: 'anywhere' }}
          >
            {data.title}
          </Typography>
          <Typography>
            Offered by{' '}
            <MuiLink
              component={Link}
              to={`/sellers/${encodeURIComponent(data.seller.userId)}`}
              sx={{ fontWeight: 700 }}
            >
              {data.seller.displayName}
            </MuiLink>
          </Typography>
        </Stack>
        <Stack direction="row" spacing={4} sx={{ flexWrap: 'wrap', rowGap: 1 }}>
          <div>
            <Typography variant="body2" color="text.secondary">
              Price range
            </Typography>
            <Typography variant="h5" component="p" sx={{ fontWeight: 700 }}>
              {formatPriceRange(data.minimumPrice, data.maximumPrice)}
            </Typography>
          </div>
          <div>
            <Typography variant="body2" color="text.secondary">
              Rating
            </Typography>
            <Rating
              average={data.averageRating}
              count={data.reviewCount}
              variant="h6"
              color="text.primary"
            />
          </div>
        </Stack>
        {canRequest && (
          <div>
            <Button variant="contained" onClick={() => setRequesting(true)}>
              Request this service
            </Button>
          </div>
        )}
        {requesting && (
          <RequestServiceDialog
            serviceId={id}
            serviceTitle={data.title}
            onClose={() => setRequesting(false)}
          />
        )}
        <Typography sx={{ whiteSpace: 'pre-line' }}>
          {data.description}
        </Typography>
        <ChatButton sellerId={data.seller.userId} />
      </Stack>
    </Paper>
  )
}

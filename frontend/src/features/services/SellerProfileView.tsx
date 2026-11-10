import { Alert, Box, CircularProgress, Stack, Typography } from '@mui/material'
import { canary, ink } from '../../theme'
import { Rating } from './Rating'
import { ServiceCard } from './ServiceCard'
import { useSellerProfile } from './useServices'

export function SellerProfileView({ userId }: { userId: string }) {
  const seller = useSellerProfile(userId)

  if (seller.isPending)
    return <CircularProgress aria-label="Loading the profile" />
  if (seller.isError) {
    return (
      <Alert severity="error">This service provider couldn't be found.</Alert>
    )
  }

  const { data } = seller
  return (
    <Stack spacing={4} sx={{ maxWidth: 1100 }}>
      <Box
        sx={{ bgcolor: canary, color: ink, maxWidth: 720, p: { xs: 3, sm: 5 } }}
      >
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={3}
          sx={{ alignItems: { sm: 'flex-start' } }}
        >
          {data.profilePhoto && (
            <Box
              component="img"
              src={data.profilePhoto}
              alt=""
              sx={{
                width: 120,
                height: 120,
                objectFit: 'cover',
                flexShrink: 0,
              }}
            />
          )}
          <Stack spacing={1.5} sx={{ minWidth: 0 }}>
            <Typography
              variant="h2"
              component="h1"
              sx={{ overflowWrap: 'anywhere' }}
            >
              {data.displayName}
            </Typography>
            {data.location && (
              <Typography sx={{ fontWeight: 700 }}>{data.location}</Typography>
            )}
            {data.bio && (
              <Typography sx={{ whiteSpace: 'pre-line' }}>
                {data.bio}
              </Typography>
            )}
            <Box
              component="dl"
              sx={{ display: 'flex', flexWrap: 'wrap', gap: 4, m: 0, pt: 1 }}
            >
              <div>
                <Typography component="dt" variant="body2">
                  Rating
                </Typography>
                <Box component="dd" sx={{ m: 0 }}>
                  <Rating
                    average={data.averageRating}
                    count={data.reviewCount}
                    variant="body1"
                    color="inherit"
                  />
                </Box>
              </div>
              <div>
                <Typography component="dt" variant="body2">
                  Completed orders
                </Typography>
                <Typography component="dd" sx={{ m: 0 }}>
                  {data.completedOrders}
                </Typography>
              </div>
            </Box>
          </Stack>
        </Stack>
      </Box>

      <Stack spacing={2}>
        <Typography variant="h3" component="h2">
          Services
        </Typography>
        {data.services.length === 0 ? (
          <Typography>
            {data.displayName} has no services on offer right now.
          </Typography>
        ) : (
          <Box
            component="ul"
            sx={{
              listStyle: 'none',
              p: 0,
              m: 0,
              display: 'grid',
              gap: 2,
              gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))',
            }}
          >
            {data.services.map((service) => (
              <li key={service.id}>
                <ServiceCard service={service} />
              </li>
            ))}
          </Box>
        )}
      </Stack>
    </Stack>
  )
}

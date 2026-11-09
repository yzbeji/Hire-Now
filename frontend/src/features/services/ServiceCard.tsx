import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Stack,
  Typography,
} from '@mui/material'
import { Link } from 'react-router'
import { formatPriceRange } from './format'
import { Rating } from './Rating'
import type { ServiceSummary } from './useServices'

type ServiceCardProps = {
  service: ServiceSummary
  /** The heading level of the card title, so it fits the page's outline. */
  headingLevel?: 'h2' | 'h3' | 'h4'
}

/** One service as a card, linking to its page. Used on seller profiles and in search results. */
export function ServiceCard({
  service,
  headingLevel = 'h3',
}: ServiceCardProps) {
  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      <CardActionArea
        component={Link}
        to={`/services/${service.id}`}
        sx={{
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'stretch',
        }}
      >
        <Cover service={service} />
        <CardContent sx={{ flexGrow: 1 }}>
          <Stack spacing={0.5}>
            <Typography
              variant="subtitle1"
              component={headingLevel}
              sx={{
                fontWeight: 700,
                lineHeight: 1.3,
                overflowWrap: 'anywhere',
              }}
            >
              {service.title}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {service.category}
            </Typography>
            <Typography sx={{ fontWeight: 700 }}>
              {formatPriceRange(service.minimumPrice, service.maximumPrice)}
            </Typography>
            <Rating
              average={service.averageRating}
              count={service.reviewCount}
            />
          </Stack>
        </CardContent>
      </CardActionArea>
    </Card>
  )
}

// Until photos exist (HN-08), the cover shows the category instead of an empty box.
function Cover({ service }: { service: ServiceSummary }) {
  if (service.coverImageUrl) {
    return (
      <Box
        component="img"
        src={service.coverImageUrl}
        alt=""
        sx={{
          width: '100%',
          aspectRatio: '4 / 3',
          objectFit: 'cover',
          display: 'block',
        }}
      />
    )
  }

  return (
    <Box
      aria-hidden="true"
      sx={{
        aspectRatio: '4 / 3',
        bgcolor: 'background.default',
        color: 'text.secondary',
        display: 'flex',
        alignItems: 'flex-end',
        p: 2,
      }}
    >
      <Typography
        sx={(theme) => ({
          fontFamily: theme.typography.h3.fontFamily,
          fontWeight: 800,
          fontSize: '1.75rem',
          lineHeight: 1,
        })}
      >
        {service.category}
      </Typography>
    </Box>
  )
}

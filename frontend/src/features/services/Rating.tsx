import { Typography, type TypographyProps } from '@mui/material'

type RatingProps = {
  average: number | null
  count: number
} & Pick<TypographyProps, 'variant' | 'color'>

/** "★ 4.8 (12 reviews)", or "No reviews yet" until someone has reviewed. */
export function Rating({
  average,
  count,
  variant = 'body2',
  color = 'text.secondary',
}: RatingProps) {
  if (average === null || count === 0) {
    return (
      <Typography variant={variant} color={color}>
        No reviews yet
      </Typography>
    )
  }

  return (
    <Typography variant={variant} color={color}>
      <span aria-hidden="true">★ </span>
      {average.toFixed(1)}{' '}
      <span>
        ({count} {count === 1 ? 'review' : 'reviews'})
      </span>
    </Typography>
  )
}

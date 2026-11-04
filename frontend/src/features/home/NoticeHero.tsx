import { Box, Typography } from '@mui/material'
import { canary, ink } from '../../theme'

const categories = [
  'Programming',
  'Graphic design',
  'Consulting',
  'Tutoring',
  'Photography',
  'Home repairs',
]

const perforation = `2px dashed ${ink}`

export function NoticeHero() {
  return (
    <Box
      sx={{
        maxWidth: 720,
        bgcolor: canary,
        transform: { md: 'rotate(-0.8deg)' },
        boxShadow: `0 2px 6px rgba(22, 34, 122, 0.3)`,
      }}
    >
      <Box sx={{ p: { xs: 3, sm: 5 } }}>
        <Typography variant="h1" gutterBottom>
          Find someone for the job
        </Typography>
        <Typography sx={{ fontSize: '1.125rem', maxWidth: '34em' }}>
          Programmers, designers, tutors, photographers, consultants and repair
          people offer their services here, online or near you.
        </Typography>
      </Box>
      <Box
        component="ul"
        aria-label="Service categories"
        sx={{
          display: 'grid',
          gridTemplateColumns: 'repeat(6, 1fr)',
          listStyle: 'none',
          m: 0,
          p: 0,
          borderTop: perforation,
        }}
      >
        {categories.map((category) => (
          <Box
            component="li"
            key={category}
            sx={{
              display: 'flex',
              justifyContent: 'center',
              py: 2,
              '& + li': { borderLeft: perforation },
            }}
          >
            <Typography
              variant="h3"
              component="span"
              sx={{
                fontSize: { xs: '1.25rem', sm: '1.625rem' },
                writingMode: 'vertical-rl',
                transform: 'rotate(180deg)',
                whiteSpace: 'nowrap',
              }}
            >
              {category}
            </Typography>
          </Box>
        ))}
      </Box>
    </Box>
  )
}

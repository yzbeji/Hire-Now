import { Alert, Box, Button, Stack, Typography } from '@mui/material'
import { canary, ink, pink } from '../../theme'
import { useChooseRole } from './useChooseRole'

const choices = [
  {
    role: 'Seller',
    label: 'Service provider',
    description: 'Offer your services and find new clients.',
    paper: canary,
  },
  {
    role: 'Buyer',
    label: 'Service seeker',
    description: 'Find and hire service providers.',
    paper: pink,
  },
] as const

export function ChooseRole() {
  const chooseRole = useChooseRole()

  return (
    <Box sx={{ maxWidth: 720 }}>
      <Typography variant="h2" component="h1" gutterBottom>
        How will you use HireNow?
      </Typography>
      <Typography sx={{ fontSize: '1.125rem', mb: 4 }}>
        You can't change this later, so pick the one that fits you.
      </Typography>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        {choices.map(({ role, label, description, paper }) => (
          <Button
            key={role}
            disabled={chooseRole.isPending}
            onClick={() => chooseRole.mutate(role)}
            sx={{
              flex: 1,
              display: 'block',
              textAlign: 'left',
              p: 3,
              color: ink,
              bgcolor: paper,
              borderTop: `2px dashed ${ink}`,
              '&:hover': { bgcolor: paper, borderTopStyle: 'solid' },
            }}
          >
            <Typography
              variant="h3"
              component="span"
              sx={{ display: 'block', mb: 1 }}
            >
              {label}
            </Typography>
            <Typography component="span" sx={{ display: 'block' }}>
              {description}
            </Typography>
          </Button>
        ))}
      </Stack>
      {chooseRole.isError && (
        <Alert severity="error" sx={{ mt: 2 }}>
          Your choice wasn't saved. Try again.
        </Alert>
      )}
    </Box>
  )
}

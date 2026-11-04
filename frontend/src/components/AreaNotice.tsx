import { Box, Typography } from '@mui/material'
import type { ReactNode } from 'react'
import { canary, ink, pink } from '../theme'

const tones = {
  provider: { bgcolor: canary, color: ink },
  seeker: { bgcolor: pink, color: ink },
  admin: { bgcolor: ink, color: '#fff' },
}

type AreaNoticeProps = {
  title: string
  tone: keyof typeof tones
  children: ReactNode
}

export function AreaNotice({ title, tone, children }: AreaNoticeProps) {
  return (
    <Box sx={{ ...tones[tone], maxWidth: 720, p: { xs: 3, sm: 5 } }}>
      <Typography variant="h2" component="h1" gutterBottom>
        {title}
      </Typography>
      <Typography sx={{ fontSize: '1.125rem' }}>{children}</Typography>
    </Box>
  )
}

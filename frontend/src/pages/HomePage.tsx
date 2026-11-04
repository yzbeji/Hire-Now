import { Stack } from '@mui/material'
import { NoticeHero } from '../features/home/NoticeHero'
import { ApiStatus } from '../features/health/ApiStatus'

export function HomePage() {
  return (
    <Stack spacing={4}>
      <NoticeHero />
      <ApiStatus />
    </Stack>
  )
}

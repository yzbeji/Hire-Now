import { AuthStatus } from '../features/auth/AuthStatus'
import { ApiStatus } from '../features/health/ApiStatus'

export function HomePage() {
  return (
    <main>
      <h1>HireNow</h1>
      <ApiStatus />
      <AuthStatus />
    </main>
  )
}

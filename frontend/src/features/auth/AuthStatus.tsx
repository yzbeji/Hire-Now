import { keycloak } from './keycloak'
import { useAuth } from './useAuth'
import { useCurrentUser } from './useCurrentUser'

export function AuthStatus() {
  const { isReady, isAuthenticated } = useAuth()
  const currentUser = useCurrentUser()

  if (!isReady) {
    return <p>Checking sign-in…</p>
  }

  if (!isAuthenticated) {
    return (
      <button type="button" onClick={() => keycloak.login()}>
        Sign in
      </button>
    )
  }

  return (
    <>
      {currentUser.data && (
        <p>
          Signed in as {currentUser.data.username} (
          {currentUser.data.roles.join(', ')})
        </p>
      )}
      {currentUser.isError && (
        <>
          <p role="alert">Could not load your account.</p>
          <button type="button" onClick={() => keycloak.login()}>
            Sign in again
          </button>
        </>
      )}
      <button
        type="button"
        onClick={() => keycloak.logout({ redirectUri: window.location.origin })}
      >
        Sign out
      </button>
    </>
  )
}

import { Button, Stack, Typography } from '@mui/material'
import { keycloak } from './keycloak'
import { useAuth } from './useAuth'
import { useCurrentUser } from './useCurrentUser'

export function AuthStatus() {
  const { isReady, isAuthenticated } = useAuth()
  const currentUser = useCurrentUser()

  if (!isReady) {
    return <Typography variant="body2">Checking sign-in…</Typography>
  }

  if (!isAuthenticated) {
    return (
      <Stack direction="row" spacing={1}>
        <Button color="inherit" onClick={() => keycloak.login()}>
          Sign in
        </Button>
        <Button
          variant="contained"
          color="secondary"
          onClick={() => keycloak.register()}
        >
          Register
        </Button>
      </Stack>
    )
  }

  const { data: user } = currentUser

  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
      {user && (
        <Button
        color="inherit"
        sx={{ flexShrink: 0 }}
        onClick={() =>  window.location.href = `${window.location.origin}/profile`} //assuming we dont still need keycloack
        >
        
        <Typography variant="body2" sx={{ overflowWrap: 'anywhere' }}>
          
          Signed in as <b>{user.username}</b>
          {user.roles.length > 0 && ` (${user.roles.join(', ')})`}
        </Typography>
        
        </Button>
      )}
      {currentUser.isError && (
        <>
          <Typography role="alert" variant="body2">
            Could not load your account.
          </Typography>
          <Button color="inherit" onClick={() => keycloak.login()}>
            Sign in again
          </Button>
        </>
      )}
      <Button
        color="inherit"
        sx={{ flexShrink: 0 }}
        onClick={() => keycloak.logout({ redirectUri: window.location.origin })}
      >
        Sign out
      </Button>
    </Stack>
  )
}

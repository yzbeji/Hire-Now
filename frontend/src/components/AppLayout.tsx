import { AppBar, Box, Button, Container, Toolbar } from '@mui/material'
import { Link, Outlet } from 'react-router'
import { AuthStatus } from '../features/auth/AuthStatus'
import { ChooseRole } from '../features/auth/ChooseRole'
import { RoleNav } from '../features/auth/RoleNav'
import { useCurrentUser } from '../features/auth/useCurrentUser'

export function AppLayout() {
  const { data: user } = useCurrentUser()

  return (
    <>
      <AppBar position="static" elevation={0}>
        <Toolbar sx={{ flexWrap: 'wrap', gap: 1, py: 1 }}>
          <Button
            component={Link}
            to="/"
            color="inherit"
            sx={(theme) => ({ ...theme.typography.h3, p: 0, mr: 2 })}
          >
            HireNow
          </Button>
          <RoleNav />
          <Box sx={{ flexGrow: 1 }} />
          <AuthStatus />
        </Toolbar>
      </AppBar>
      <Container component="main" sx={{ py: { xs: 3, sm: 6 } }}>
        {user?.roles.length === 0 ? <ChooseRole /> : <Outlet />}
      </Container>
    </>
  )
}

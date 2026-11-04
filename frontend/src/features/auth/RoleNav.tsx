import { Button, Stack } from '@mui/material'
import { NavLink } from 'react-router'
import { useCurrentUser, type Role } from './useCurrentUser'

const links: { to: string; label: string; role: Role }[] = [
  { to: '/seller', label: 'Seller area', role: 'Seller' },
  { to: '/buyer', label: 'Buyer area', role: 'Buyer' },
  { to: '/admin', label: 'Admin area', role: 'Admin' },
]

export function RoleNav() {
  const { data: user } = useCurrentUser()

  return (
    <Stack component="nav" aria-label="Main" direction="row" spacing={1}>
      {links
        .filter(({ role }) => user?.roles.includes(role))
        .map(({ to, label }) => (
          <Button
            key={to}
            component={NavLink}
            to={to}
            color="inherit"
            sx={{
              '&.active': {
                textDecoration: 'underline',
                textUnderlineOffset: 6,
              },
            }}
          >
            {label}
          </Button>
        ))}
    </Stack>
  )
}

import { useState } from 'react'
import { Alert, Button, Collapse, InputAdornment, Stack, TextField, Typography } from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import { Link, useLocation } from 'react-router'
import { useCurrentUser } from '../features/auth/useCurrentUser'
import { MyListings } from '../features/listings/MyListings'
import { emptyFilters, type ListingFilters } from '../features/listings/listingFilters'

export function MyListingsPage() {
  const { data: me } = useCurrentUser()
  const notice = (useLocation().state as { notice?: string } | null)?.notice
  const [searchOpen, setSearchOpen] = useState(false)
  const [filters, setFilters] = useState<ListingFilters>(emptyFilters)

  const set = (key: keyof ListingFilters) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setFilters((prev) => ({ ...prev, [key]: e.target.value }))

  return (
    <Stack spacing={3} sx={{ maxWidth: 960 }}>
      <Stack
        direction="row"
        spacing={2}
        sx={{
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          rowGap: 2,
        }}
      >
        <Typography variant="h2" component="h1">
          My services
        </Typography>
        <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', rowGap: 2 }}>
          {me && (
            <Button
              component={Link}
              to={`/sellers/${encodeURIComponent(me.id)}`}
              variant="outlined"
            >
              View my public profile
            </Button>
          )}
          <Button
            variant="outlined"
            startIcon={<SearchIcon />}
            onClick={() => setSearchOpen((o) => !o)}
          >
            Search services
          </Button>
          <Button component={Link} to="/seller/listings/new" variant="contained">
            Create a listing
          </Button>
        </Stack>
      </Stack>

      <Collapse in={searchOpen}>
        <Stack spacing={2}>
          <TextField
            label="Search services"
            placeholder="Keywords or first letters…"
            value={filters.keywords}
            onChange={set('keywords')}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
              },
            }}
          />
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField label="Min price" type="number" value={filters.minPrice} onChange={set('minPrice')} />
            <TextField label="Max price" type="number" value={filters.maxPrice} onChange={set('maxPrice')} />
            <TextField label="Location" value={filters.location} onChange={set('location')} fullWidth />
          </Stack>
          <Button onClick={() => setFilters(emptyFilters)} sx={{ alignSelf: 'flex-start' }}>
            Clear filters
          </Button>
        </Stack>
      </Collapse>

      {notice && <Alert severity="success">{notice}</Alert>}
      <MyListings filters={filters} />
    </Stack>
  )
}
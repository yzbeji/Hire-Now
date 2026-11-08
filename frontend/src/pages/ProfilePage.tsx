import { Alert, Button, Card, CardContent, Stack, TextField, Typography } from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { apiGet, apiPut } from '../api/client'
import { useAuth } from '../features/auth/useAuth'
import { useCurrentUser } from '../features/auth/useCurrentUser'

type Profile = {
  userId: string
  displayName: string
  bio: string
  location: string
  profilePhoto: string
}

type ProfileForm = {
  displayName: string
  bio: string
  location: string
  profilePhoto: string
}

const emptyProfile: ProfileForm = {
  displayName: '',
  bio: '',
  location: '',
  profilePhoto: '',
}

export function DisplayProfile(id: string | null) {
  const { isReady, isAuthenticated } = useAuth()
  const { data: currentUser } = useCurrentUser()
  const queryClient = useQueryClient()
    const [form, setForm] = useState<ProfileForm>(emptyProfile)

  const isOwnProfile = !id && isAuthenticated
   const { data: profile, isLoading } = useQuery<Profile>({
    queryKey: ['profile', id ?? 'me'],
    queryFn: async () => {
      if (isOwnProfile) {
        return apiGet<Profile>('/me/profile')
      }

      if (id) {
        return apiGet<Profile>(`/profiles/${id}`)
      }

      throw new Error('No profile selected')
    },
    enabled: isReady && (!!id || isAuthenticated),
  })

  // Refills the form whenever a different profile loads, during render rather than in an effect.
  const [loadedProfile, setLoadedProfile] = useState(profile)
  if (profile !== loadedProfile) {
    setLoadedProfile(profile)
    setForm(
      profile
        ? {
            displayName: profile.displayName ?? '',
            bio: profile.bio ?? '',
            location: profile.location ?? '',
            profilePhoto: profile.profilePhoto ?? '',
          }
        : emptyProfile,
    )
  }

  const saveProfile = useMutation({
    mutationFn: (values: ProfileForm) =>
      apiPut<Profile>('/me/profile', {
        displayName: values.displayName,
        bio: values.bio,
        location: values.location,
        profilePhoto: values.profilePhoto,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] })
      queryClient.invalidateQueries({ queryKey: ['me'] })
    },
  })

  if (!isReady) {
    return <Typography variant="body2">Checking sign-in…</Typography>
  }

  if (!isAuthenticated && !id) {
    return <Typography variant="body2">No profile found...</Typography>
  }

  if (isOwnProfile) {
    return (
      <Card sx={{ maxWidth: 760, mx: 'auto', mt: 4, p: 1 }}>
        <CardContent>
          <Stack spacing={3}>
            <Typography variant="h3">Your profile</Typography>
            <TextField
              label="Display name"
              value={form.displayName}
              onChange={(event) => setForm((prev) => ({ ...prev, displayName: event.target.value }))}
            />
            <TextField
              label="Bio"
              multiline
              minRows={4}
              slotProps={{ htmlInput: { maxLength: 12 } }}
              value={form.bio}
              onChange={(event) => setForm((prev) => ({ ...prev, bio: event.target.value }))}
            />
            <TextField
              label="Location"
              value={form.location}
              onChange={(event) => setForm((prev) => ({ ...prev, location: event.target.value }))}
            />
            <TextField
              label="Profile photo URL"
              value={form.profilePhoto}
              onChange={(event) => setForm((prev) => ({ ...prev, profilePhoto: event.target.value }))}
            />
            <Button
              variant="contained"
              onClick={() => saveProfile.mutate(form)}
              disabled={saveProfile.isPending || !form.displayName.trim()}
            >
              {saveProfile.isPending ? 'Saving…' : 'Save profile'}
            </Button>
            {saveProfile.isError && (
              <Alert severity="error">We could not save your profile. Please try again.</Alert>
            )}
            {saveProfile.isSuccess && (
              <Alert severity="success">Your profile was updated.</Alert>
            )}
            {isLoading && <Typography variant="body2">Loading your profile…</Typography>}
          </Stack>
        </CardContent>
      </Card>
    )
  }

  if (!profile) {
    return <Typography variant="body2">No profile found...</Typography>
  }

  return (
    <Card sx={{ maxWidth: 760, mx: 'auto', mt: 4, p: 1 }}>
      <CardContent>
        <Stack spacing={2}>
          <Typography variant="h3">{profile.displayName || 'Profile'}</Typography>
          {profile.profilePhoto && (
            <img
              src={profile.profilePhoto}
              alt={`${profile.displayName || 'Profile'} photo`}
              style={{ width: 160, height: 160, objectFit: 'cover', borderRadius: 8 }}
            />
          )}
          <Typography variant="body1">{profile.location || 'Location not set'}</Typography>
          <Typography variant="body1">{profile.bio || 'No bio yet.'}</Typography>
          {currentUser && currentUser.roles.includes('Buyer') && profile.userId !== currentUser.id && (
            <Alert severity="info">This profile is only visible to signed-in sellers.</Alert>
          )}
        </Stack>
      </CardContent>
    </Card>
  )
}


export function ProfilePage() {
  return(
    DisplayProfile(useParams().id ?? null)
  )
}

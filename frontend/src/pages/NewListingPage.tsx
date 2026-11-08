import {
  Alert,
  Button,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { apiPost } from '../api/client'

const categories = [
  
  'Programming',
  'Graphic Design',
  'Consulting',
  'Tutoring',
  'Photography',
  'Home Repairs',
  'Other',
]

type ListingStatus = 'Draft' | 'Published'

type ListingForm = {
  title: string
  description: string
  category: string
  minimumPrice: string
  maximumPrice: string
  status: ListingStatus
}

type FieldErrors = Partial<Record<keyof ListingForm, string>>

const emptyForm: ListingForm = {
  title: '',
  description: '',
  category: '',
  minimumPrice: '',
  maximumPrice: '',
  status: 'Draft',
}

function validateListing(form: ListingForm): FieldErrors {
  const errors: FieldErrors = {}
  const title = form.title.trim()
  if (!title) {
    errors.title = 'Title is required.'
  } else if (title.length > 100) {
    errors.title = 'Title must be 100 characters or fewer.'
  }

  const description = form.description.trim()
  if (!description) {
    errors.description = 'Description is required.'
  } else if (description.length > 5000) {
    errors.description = 'Description must be 5000 characters or fewer.'
  }

  if (!form.category) {
    errors.category = 'Category is required.'
  }

  const minimumPrice = Number(form.minimumPrice)
  const maximumPrice = Number(form.maximumPrice)

  if (form.minimumPrice.trim() === '') {
    errors.minimumPrice = 'Minimum price is required.'
  } else if (Number.isNaN(minimumPrice) || minimumPrice <= 0) {
    errors.minimumPrice = 'Minimum price must be greater than 0.'
  }

  if (form.maximumPrice.trim() === '') {
    errors.maximumPrice = 'Maximum price is required.'
  } else if (Number.isNaN(maximumPrice) || maximumPrice <= 0) {
    errors.maximumPrice = 'Maximum price must be greater than 0.'
  }

  if (
    !Number.isNaN(minimumPrice) &&
    !Number.isNaN(maximumPrice) &&
    minimumPrice > 0 &&
    maximumPrice > 0 &&
    minimumPrice > maximumPrice
  ) {
    errors.minimumPrice = "Minimum price can't exceed maximum price."
  }

  return errors
}

export function NewListingPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState<ListingForm>(emptyForm)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [serverMessage, setServerMessage] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const updateField = <K extends keyof ListingForm>(key: K, value: ListingForm[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
    setErrors((current) => ({ ...current, [key]: undefined }))
  }

  async function handleSubmit(nextStatus: ListingStatus) {
    const nextForm = { ...form, status: nextStatus }
    const nextErrors = validateListing(nextForm)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) {
      setServerMessage('')
      return
    }

    setSubmitting(true)
    setServerMessage('')

    try {
      await apiPost('/me/listings', {
        title: nextForm.title.trim(),
        description: nextForm.description.trim(),
        category: nextForm.category,
        minimumPrice: Number(nextForm.minimumPrice),
        maximumPrice: Number(nextForm.maximumPrice),
        status: nextStatus,
      })

      setServerMessage(
        nextStatus === 'Published'
          ? 'Your listing is live and visible to buyers.'
          : 'Your listing draft was saved.',
      )
      setForm(emptyForm)
      setErrors({})

      if (nextStatus === 'Published') {
        navigate('/seller')
      }
    } catch (error) {
      const payload = (error as Error & { payload?: Record<string, string[] | string> }).payload
      const mapped = Object.fromEntries(
        Object.entries(payload ?? {}).flatMap(([key, value]) => {
          const values = Array.isArray(value) ? value : [value]
          return values.length > 0 ? [[key, values[0]]] : []
        }),
      )
      setErrors(mapped as FieldErrors)
      setServerMessage('Please fix the highlighted fields and try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Card sx={{ maxWidth: 860, mx: 'auto', p: 1 }}>
      <CardContent>
        <Stack spacing={3}>
          <Stack direction="row"  spacing={2}>
            <Typography variant="h3">Create a listing</Typography>
            <Button component={Link} to="/seller" variant="outlined">
              Back to seller area
            </Button>
          </Stack>

          {serverMessage && (
            <Alert severity={serverMessage.includes('live') || serverMessage.includes('saved') ? 'success' : 'error'}>
              {serverMessage}
            </Alert>
          )}

          <TextField
            label="Title"
            value={form.title}
            onChange={(event) => updateField('title', event.target.value)}
            error={Boolean(errors.title)}
            helperText={errors.title ?? ' '}
            slotProps={{ htmlInput: { maxLength: 100 } }}
          />

          <TextField
            label="Description"
            value={form.description}
            onChange={(event) => updateField('description', event.target.value)}
            error={Boolean(errors.description)}
            helperText={errors.description ?? ' '}
            multiline
            minRows={6}
            slotProps={{ htmlInput: { maxLength: 5000 } }}
          />

          <FormControl fullWidth error={Boolean(errors.category)}>
            <InputLabel id="listing-category-label">Category</InputLabel>
            <Select
              labelId="listing-category-label"
              label="Category"
              value={form.category}
              onChange={(event) => updateField('category', String(event.target.value))}
            >
              {categories.map((category) => (
                <MenuItem key={category} value={category}>
                  {category}
                </MenuItem>
              ))}
            </Select>
            {errors.category && (
              <Typography variant="caption" color="error">
                {errors.category}
              </Typography>
            )}
          </FormControl>

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField
              label="Minimum price"
              type="number"
              value={form.minimumPrice}
              onChange={(event) => updateField('minimumPrice', event.target.value)}
              error={Boolean(errors.minimumPrice)}
              helperText={errors.minimumPrice ?? ' '}
              fullWidth
              slotProps={{ htmlInput: { minLength: 1 } }}
            />
            <TextField
              label="Maximum price"
              type="number"
              value={form.maximumPrice}
              onChange={(event) => updateField('maximumPrice', event.target.value)}
              error={Boolean(errors.maximumPrice)}
              helperText={errors.maximumPrice ?? ' '}
              fullWidth
              slotProps={{ htmlInput: { minLength: 1 } }}
            />
          </Stack>

          <Stack direction="row" spacing={2} >
            <Button variant="outlined" onClick={() => handleSubmit('Draft')} disabled={submitting}>
              {submitting ? 'Saving…' : 'Save draft'}
            </Button>
            <Button variant="contained" onClick={() => handleSubmit('Published')} disabled={submitting}>
              {submitting ? 'Publishing…' : 'Publish'}
            </Button>
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  )
}

import {
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
} from '@mui/material'
import type { ReactNode } from 'react'
import type { ListingFormErrors, ListingFormValues } from './listingFormValues'
import { useCategories } from './useListings'

type ListingFormProps = {
  values: ListingFormValues
  errors: ListingFormErrors
  onChange: (
    values: ListingFormValues,
    changed: keyof ListingFormValues,
  ) => void
  actions: ReactNode
}

export function ListingForm({
  values,
  errors,
  onChange,
  actions,
}: ListingFormProps) {
  const categories = useCategories()
  const names = categories.data?.map((category) => category.name) ?? []
  // Keeps a listing's current category selectable even if it is no longer offered.
  if (values.category && !names.includes(values.category))
    names.push(values.category)

  const set = (key: keyof ListingFormValues) => (value: string) =>
    onChange({ ...values, [key]: value }, key)

  return (
    <Stack spacing={3}>
      <TextField
        label="Title"
        value={values.title}
        onChange={(event) => set('title')(event.target.value)}
        error={Boolean(errors.title)}
        helperText={errors.title ?? ' '}
        slotProps={{ htmlInput: { maxLength: 100 } }}
      />

      <TextField
        label="Description"
        value={values.description}
        onChange={(event) => set('description')(event.target.value)}
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
          value={categories.isPending ? '' : values.category}
          onChange={(event) => set('category')(String(event.target.value))}
        >
          {names.map((name) => (
            <MenuItem key={name} value={name}>
              {name}
            </MenuItem>
          ))}
        </Select>
        <FormHelperText>{errors.category ?? ' '}</FormHelperText>
      </FormControl>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <TextField
          label="Minimum price"
          type="number"
          value={values.minimumPrice}
          onChange={(event) => set('minimumPrice')(event.target.value)}
          error={Boolean(errors.minimumPrice)}
          helperText={errors.minimumPrice ?? ' '}
          fullWidth
          slotProps={{ htmlInput: { min: 1 } }}
        />
        <TextField
          label="Maximum price"
          type="number"
          value={values.maximumPrice}
          onChange={(event) => set('maximumPrice')(event.target.value)}
          error={Boolean(errors.maximumPrice)}
          helperText={errors.maximumPrice ?? ' '}
          fullWidth
          slotProps={{ htmlInput: { min: 1 } }}
        />
      </Stack>

      <Stack direction="row" spacing={2}>
        {actions}
      </Stack>
    </Stack>
  )
}

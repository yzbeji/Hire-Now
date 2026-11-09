import { useState } from 'react'
import {Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField,} from '@mui/material'
import { useCreateRequest } from './useRequests'

type Props = { serviceId: number; serviceTitle: string; onClose: () => void }

export function RequestServiceDialog({ serviceId, serviceTitle, onClose }: Props) {
    const create = useCreateRequest(serviceId)
    const [message, setMessage] = useState('')
    const [budget, setBudget] = useState('')
    const [preferredDate, setPreferredDate] = useState('')

    const submit = () =>
    create.mutate(
        {
        message: message.trim(),
        budget: budget === '' ? null : Number(budget),
        preferredDate: preferredDate || null,
        },
        { onSuccess: () => {} }, 
    )

    return (
    <Dialog open onClose={onClose} fullWidth maxWidth="sm" aria-labelledby="request-title">
        <DialogTitle id="request-title">Request "{serviceTitle}"</DialogTitle>
        <DialogContent>
        {create.isSuccess ? (
            <Alert severity="success" sx={{ mt: 1 }}>
            Your request was sent. You can follow it in your buyer area.
            </Alert>
        ) : (
            <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
                label="What do you need?"
                multiline
                minRows={4}
                required
                value={message}
                onChange={(e) => setMessage(e.target.value)}
            />
            <TextField
                label="Your budget (optional)"
                type="number"
                value={budget}
                onChange={(e) => setBudget(e.target.value)}
            />
            <TextField
                label="Preferred date (optional)"
                type="date"
                value={preferredDate}
                onChange={(e) => setPreferredDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
            />
            {create.isError && (
                <Alert severity="error">Your request wasn't sent. Please try again.</Alert>
            )}
            </Stack>
        )}
        </DialogContent>
        <DialogActions>
        <Button onClick={onClose}>{create.isSuccess ? 'Close' : 'Cancel'}</Button>
        {!create.isSuccess && (
            <Button
            variant="contained"
            onClick={submit}
            disabled={create.isPending || !message.trim()}
            >
            {create.isPending ? 'Sending…' : 'Send request'}
            </Button>
        )}
        </DialogActions>
    </Dialog>
    )
}
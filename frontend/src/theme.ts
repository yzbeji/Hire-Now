import { createTheme } from '@mui/material'

// Tokens are documented in DESIGN.md at the repository root.
export const ink = '#16227A'
export const canary = '#FFD84D'
export const pink = '#FFC2CF'
const board = '#E4E8F2'

const display = '"Big Shoulders Display Variable", "Arial Narrow", sans-serif'
const heading = { fontFamily: display, fontWeight: 800, lineHeight: 1 }

export const theme = createTheme({
  palette: {
    primary: { main: ink },
    secondary: { main: canary, contrastText: ink },
    background: { default: board },
    text: { primary: ink, secondary: '#3B4590' },
  },
  shape: { borderRadius: 3 },
  typography: {
    fontFamily: '"Atkinson Hyperlegible Next Variable", Verdana, sans-serif',
    h1: { ...heading, fontSize: 'clamp(3.5rem, 11vw, 7rem)' },
    h2: { ...heading, fontSize: 'clamp(2.75rem, 7vw, 4.5rem)' },
    h3: { ...heading, fontSize: '2rem' },
    button: { textTransform: 'none', fontWeight: 700, fontSize: '1rem' },
  },
  components: {
    // The default ink focus outline would disappear against the ink bar.
    MuiAppBar: {
      styleOverrides: {
        root: {
          '& .Mui-focusVisible.Mui-focusVisible': { outlineColor: '#fff' },
        },
      },
    },
    MuiButton: { defaultProps: { disableElevation: true } },
    MuiButtonBase: {
      styleOverrides: {
        root: {
          '&.Mui-focusVisible': {
            outline: `3px solid ${ink}`,
            outlineOffset: 2,
          },
        },
      },
    },
  },
})

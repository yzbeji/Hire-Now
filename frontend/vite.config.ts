import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // File events don't reach a container through a Windows or macOS bind mount.
    watch: { usePolling: process.env.VITE_USE_POLLING === 'true' },
  },
})

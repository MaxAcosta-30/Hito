import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// En desarrollo, /api/* va a la API local igual que en Vercel: se quita el prefijo /api.
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5186',
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
})

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const ambiente = process.env.FICHA_AMBIENTE ?? 'demo'
if (ambiente !== 'demo' && ambiente !== 'estudio') {
  throw new Error('FICHA_AMBIENTE deve ser demo ou estudio.')
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  define: {
    __FICHA_AMBIENTE__: JSON.stringify(ambiente),
  },
  build: {
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:5057',
        changeOrigin: true,
      },
    },
  },
})

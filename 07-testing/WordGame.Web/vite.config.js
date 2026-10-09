import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  // The API serves the built app, so the E2E tests only need to start one process
  build: {
    outDir: '../WordGame.Api/wwwroot',
    emptyOutDir: true,
  },
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5076',
    },
  },
})

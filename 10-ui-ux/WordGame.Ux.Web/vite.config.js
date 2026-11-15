import { resolve } from 'node:path'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The examples embedded in the lecture slides: both versions of the game and a fake
// server in ONE script file. assets/ux-example.lua inlines it into the slides, where
// every example runs in its own iframe - no API and no network needed.
export default defineConfig({
  plugins: [react()],
  resolve: {
    dedupe: ['react', 'react-dom'],
  },
  define: {
    'process.env.NODE_ENV': '"production"',
  },
  build: {
    outDir: 'dist-examples',
    emptyOutDir: true,
    lib: {
      entry: resolve(import.meta.dirname, 'src/examples/main.jsx'),
      formats: ['iife'],
      name: 'UxExamples',
      fileName: () => 'examples.js',
    },
  },
})

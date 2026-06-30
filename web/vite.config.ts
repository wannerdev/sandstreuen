import { defineConfig } from 'vite';

export default defineConfig({
  base: './',
  build: {
    target: 'es2022',
  },
  server: {
    // Native file events are unreliable on WSL2 / Windows-mounted drives
    // (/mnt/c), so HMR can miss edits. Polling makes the watcher dependable.
    watch: { usePolling: true, interval: 200 },
  },
});

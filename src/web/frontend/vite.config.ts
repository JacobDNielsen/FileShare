import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import fs from 'node:fs'
import path from 'node:path'

export default defineConfig(() => {
  const certPath = path.resolve(__dirname, '../../.dev-certs/pem')
  const certFile = path.join(certPath, 'fileshare-dev.crt')
  const keyFile = path.join(certPath, 'fileshare-dev.key')
  const useSsl = fs.existsSync(certFile) && fs.existsSync(keyFile)

  const httpsConfig = useSsl
    ? {
        cert: fs.readFileSync(certFile),
        key: fs.readFileSync(keyFile),
      }
    : undefined

  const gatewayTarget = useSsl
    ? 'https://localhost:8089'
    : 'http://localhost:8088'

  return {
    plugins: [react()],
    server: {
      https: httpsConfig,
      port: 5173,
      proxy: {
        '/api': {
          target: gatewayTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})

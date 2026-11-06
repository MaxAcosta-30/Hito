import { defineConfig, devices } from '@playwright/test'

// Prueba la pila real: navegador → Vite (proxy /api) → API .NET → PostgreSQL.
// La base debe existir y tener las migraciones aplicadas antes de correr (ver docs/proyecto/05-calidad.md).
const conexion = process.env.E2E_CONNECTION_STRING
if (!conexion) throw new Error('Define E2E_CONNECTION_STRING con una base PostgreSQL desechable.')

export default defineConfig({
  testDir: '.',
  timeout: 60_000,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  // La vista del cliente se diseñó primero para móvil: el flujo corre en un teléfono.
  projects: [{ name: 'movil', use: { ...devices['Pixel 7'] } }],
  webServer: [
    {
      command: 'dotnet run --project ../../src/Hito.Api --no-launch-profile',
      url: 'http://localhost:5186/health/ready',
      timeout: 180_000,
      reuseExistingServer: !process.env.CI,
      env: {
        ASPNETCORE_URLS: 'http://localhost:5186',
        ConnectionStrings__Hito: conexion,
        // Clave solo para esta prueba: los tokens mueren con el proceso.
        Jwt__Clave: 'clave-de-prueba-e2e-no-es-secreta-0000000',
      },
    },
    {
      command: 'npm run dev -- --port 5173 --strictPort',
      cwd: '../../web',
      url: 'http://localhost:5173',
      timeout: 60_000,
      reuseExistingServer: !process.env.CI,
    },
  ],
})

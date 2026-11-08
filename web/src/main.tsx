import '@fontsource/bricolage-grotesque/800.css'
import '@fontsource/public-sans/400.css'
import '@fontsource/public-sans/600.css'
import '@fontsource/jetbrains-mono/500.css'
import '@fontsource/jetbrains-mono/700.css'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router'
import App from './App.tsx'
import { TintaDefs } from './componentes/Tinta'
import './index.css'

// 30 s de frescura: lo precargado al pasar el cursor no se vuelve a pedir al abrir la pantalla.
// Las mutaciones invalidan sus consultas, así que lo que cambia el usuario se ve al instante.
const queryClient = new QueryClient({ defaultOptions: { queries: { staleTime: 30_000 } } })

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        {/* Un solo filtro de tinta para todos los sellos de la app */}
        <TintaDefs />
        <App />
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
)

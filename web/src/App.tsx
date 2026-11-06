import { useEffect, useState } from 'react'
import { esperarApi } from './api/esperarApi'

type Estado = 'despertando' | 'lista' | 'sin-base' | 'error'

const MENSAJES: Record<Estado, string> = {
  despertando: 'Despertando la demo, puede tardar hasta un minuto',
  lista: 'API y base de datos conectadas',
  'sin-base': 'La API responde, pero la base de datos no está disponible',
  error: 'No se pudo despertar la demo',
}

export default function App() {
  const [estado, setEstado] = useState<Estado>('despertando')
  const [intento, setIntento] = useState(0)

  useEffect(() => {
    const control = new AbortController()
    ;(async () => {
      if (!(await esperarApi('/api/health/live', control.signal))) {
        if (!control.signal.aborted) setEstado('error')
        return
      }
      const res = await fetch('/api/health/ready', { signal: control.signal, cache: 'no-store' }).catch(() => null)
      if (!control.signal.aborted) setEstado(res?.ok ? 'lista' : 'sin-base')
    })()
    return () => control.abort()
  }, [intento])

  return (
    <main>
      <h1>Hito</h1>
      <p>Cada entrega aprobada, cada cobro claro.</p>
      <p role="status" aria-live="polite" data-estado={estado}>
        {MENSAJES[estado]}
      </p>
      {estado === 'error' && <button onClick={() => { setEstado('despertando'); setIntento((n) => n + 1) }}>Reintentar</button>}
    </main>
  )
}

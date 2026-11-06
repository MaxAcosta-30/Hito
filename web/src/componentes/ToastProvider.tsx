import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { ToastContexto } from './toast'

// Éxito: un aviso breve. La región vive siempre montada para que el lector de pantalla lo anuncie.
export function ToastProvider({ children }: { children: ReactNode }) {
  const [mensaje, setMensaje] = useState<{ texto: string; id: number } | null>(null)

  useEffect(() => {
    if (!mensaje) return
    const temporizador = setTimeout(() => setMensaje(null), 3500)
    return () => clearTimeout(temporizador)
  }, [mensaje])

  const mostrar = useCallback((texto: string) => setMensaje({ texto, id: Date.now() }), [])

  return (
    <ToastContexto.Provider value={mostrar}>
      {children}
      <div className="toast-region" role="status" aria-live="polite">
        {mensaje && (
          <div key={mensaje.id} className="toast">
            {mensaje.texto}
          </div>
        )}
      </div>
    </ToastContexto.Provider>
  )
}

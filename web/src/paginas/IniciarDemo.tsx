import { useMutation } from '@tanstack/react-query'
import { useEffect, useRef } from 'react'
import { Link, Navigate, useNavigate } from 'react-router'
import { api, ErrorApi, type Sesion } from '../api/cliente'
import { useSesion } from '../auth/contexto'

// /demo: el botón de la landing llega aquí. La API ya está despierta (la puerta de App lo garantiza),
// así que se crea la cuenta de ejemplo una sola vez y se entra al panel.
export function IniciarDemo() {
  const { estado, iniciar } = useSesion()
  const navegar = useNavigate()
  const lanzada = useRef(false)
  const demo = useMutation({
    mutationFn: () => api<Sesion>('/demo', { method: 'POST' }),
    onSuccess: (sesion) => {
      iniciar(sesion)
      navegar('/cobros', { replace: true })
    },
  })

  useEffect(() => {
    // Un solo intento por visita: el ref sobrevive al doble efecto de StrictMode.
    if (estado.tipo !== 'anonimo' || lanzada.current) return
    lanzada.current = true
    demo.mutate()
  }, [estado.tipo, demo])

  // Quien ya tiene sesión no necesita una demo.
  if (estado.tipo === 'autenticado' && !demo.isSuccess) return <Navigate to="/cobros" replace />

  return (
    <main className="contenido angosto">
      {demo.isError ? (
        <div className="aviso aviso-error" role="alert">
          <div>
            <b>{demo.error instanceof ErrorApi ? demo.error.message : 'No pudimos crear la demo.'}</b>
            <div className="meta">Inténtalo de nuevo en un momento.</div>
            <button className="btn secundario" onClick={() => demo.mutate()}>
              Reintentar
            </button>{' '}
            <Link className="btn texto" to="/">
              Volver al inicio
            </Link>
          </div>
        </div>
      ) : (
        <div className="hoja" role="status">
          <p className="titular">Preparando tu cuenta de ejemplo…</p>
          <p className="meta">Clientes, proyectos y cobros listos para recorrer. Se borra sola en 24 horas.</p>
        </div>
      )}
    </main>
  )
}

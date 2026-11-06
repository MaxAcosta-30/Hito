import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { api, guardarToken, renovar, type Sesion } from '../api/cliente'
import { SesionContexto, type EstadoSesion } from './contexto'

export function SesionProvider({ children }: { children: ReactNode }) {
  const [estado, setEstado] = useState<EstadoSesion>({ tipo: 'cargando' })

  // Al cargar la app, la cookie de renovación (si existe) recupera la sesión sin pedir la contraseña.
  useEffect(() => {
    let activo = true
    renovar().then((sesion) => {
      if (activo) setEstado(sesion ? { tipo: 'autenticado', usuario: sesion.usuario } : { tipo: 'anonimo' })
    })
    return () => {
      activo = false
    }
  }, [])

  const iniciar = useCallback((sesion: Sesion) => {
    guardarToken(sesion.accessToken)
    setEstado({ tipo: 'autenticado', usuario: sesion.usuario })
  }, [])

  const salir = useCallback(async () => {
    await api('/auth/salir', { method: 'POST' }).catch(() => undefined)
    guardarToken(null)
    setEstado({ tipo: 'anonimo' })
  }, [])

  const valor = useMemo(() => ({ estado, iniciar, salir }), [estado, iniciar, salir])
  return <SesionContexto.Provider value={valor}>{children}</SesionContexto.Provider>
}

import { createContext, useContext } from 'react'
import type { Sesion, Usuario } from '../api/cliente'

export type EstadoSesion = { tipo: 'cargando' } | { tipo: 'anonimo' } | { tipo: 'autenticado'; usuario: Usuario }

export type ContextoSesion = {
  estado: EstadoSesion
  iniciar: (sesion: Sesion) => void
  salir: () => Promise<void>
}

export const SesionContexto = createContext<ContextoSesion | null>(null)

export function useSesion() {
  const contexto = useContext(SesionContexto)
  if (!contexto) throw new Error('useSesion debe usarse dentro de SesionProvider')
  return contexto
}

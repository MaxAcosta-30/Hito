import { useQuery } from '@tanstack/react-query'
import { api } from './cliente'

export type EstadoProyecto = 'Borrador' | 'Enviado' | 'Aceptado' | 'Completado' | 'Cancelado'
export type EstadoVersion = 'Borrador' | 'Enviada' | 'Aceptada' | 'Reemplazada'
export type EstadoHito = 'Pendiente' | 'Entregado' | 'CambiosSolicitados' | 'Aprobado' | 'Pagado'

export type Hito = {
  id: string
  orden: number
  titulo: string
  monto: number
  fechaEntrega: string
  entregable: string
  estado: EstadoHito
  evidenciaUrl: string | null
  entregadoEn: string | null
  comentarioCambios: string | null
  aprobadoEn: string | null
}

export type Version = {
  numero: number
  estado: EstadoVersion
  creadaEn: string
  enviadaEn: string | null
  aceptadaEn: string | null
  total: number
  hitos: Hito[]
}

export type ClienteResumen = { id: string; nombre: string }

export type ProyectoResumen = {
  id: string
  nombre: string
  cliente: ClienteResumen
  estado: EstadoProyecto
  total: number
  version: number
  actualizadoEn: string
}

export type Proyecto = {
  id: string
  nombre: string
  cliente: ClienteResumen
  estado: EstadoProyecto
  total: number
  versionActual: number
  ultimaVersion: number
  versiones: Version[]
  enlace: { creadoEn: string; expiraEn: string } | null
  creadoEn: string
  actualizadoEn: string
}

export type HitoFormulario = { id?: string; titulo: string; monto: number; fechaEntrega: string | null; entregable: string }

export const useProyectos = () => useQuery({ queryKey: ['proyectos'], queryFn: () => api<ProyectoResumen[]>('/proyectos') })

export const useProyecto = (id: string | undefined) =>
  useQuery({ queryKey: ['proyectos', id], queryFn: () => api<Proyecto>(`/proyectos/${id}`), enabled: !!id })

export const versionDe = (proyecto: Proyecto, numero: number) => proyecto.versiones.find((v) => v.numero === numero)!

// El token solo llega aquí, al crearlo: el servidor guarda su hash y no puede volver a mostrarlo.
export type EnlaceCreado = { token: string; expiraEn: string; proyecto: Proyecto }

export const urlDelCliente = (token: string) => `${window.location.origin}/p/${token}`

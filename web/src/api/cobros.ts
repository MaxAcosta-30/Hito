import { useQuery } from '@tanstack/react-query'
import { api } from './cliente'
import type { EstadoHito } from './proyectos'

export type MotivoDeAtencion = 'Vencido' | 'AprobadoSinPagar' | 'CambiosSolicitados' | 'EsperaRevision'

export type HitoQueAtender = {
  proyectoId: string
  proyecto: string
  cliente: string
  hitoId: string
  titulo: string
  pendiente: number
  fechaEntrega: string
  estado: EstadoHito
  motivo: MotivoDeAtencion
}

export type Panel = {
  porCobrar: number
  aprobadoSinPagar: number
  vencido: number
  proyectosActivos: number
  hoy: string
  atencion: HitoQueAtender[]
}

export const usePanel = () => useQuery({ queryKey: ['cobros'], queryFn: () => api<Panel>('/cobros') })

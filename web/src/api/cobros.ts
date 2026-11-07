import { queryOptions, useQuery } from '@tanstack/react-query'
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

// Acumulados al cierre de cada mes (el mes en curso se corta en "hoy").
export type MesDeCobro = { mes: string; acordado: number; cobrado: number }

export type Panel = {
  porCobrar: number
  aprobadoSinPagar: number
  vencido: number
  proyectosActivos: number
  hoy: string
  atencion: HitoQueAtender[]
  historial: MesDeCobro[]
}

export const consultaPanel = () => queryOptions({ queryKey: ['cobros'], queryFn: () => api<Panel>('/cobros') })

export const usePanel = () => useQuery(consultaPanel())

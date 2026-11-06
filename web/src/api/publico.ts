import { useQuery } from '@tanstack/react-query'
import { api, ErrorApi } from './cliente'
import type { EstadoHito, EstadoProyecto } from './proyectos'

export type HitoPublico = {
  id: string
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
export type VersionPublica = { numero: number; enviadaEn: string | null; aceptadaEn: string | null; total: number; hitos: HitoPublico[] }
export type VistaCliente = {
  freelancer: string
  proyecto: string
  cliente: string
  estado: EstadoProyecto
  vigente: VersionPublica | null
  porAceptar: VersionPublica | null
  enlaceExpiraEn: string
}

export const useVistaCliente = (token: string) =>
  useQuery({
    queryKey: ['publico', token],
    queryFn: () => api<VistaCliente>(`/publico/${token}`),
    // Un 404 significa enlace no válido: reintentar no lo arregla.
    retry: (intentos, error) => !(error instanceof ErrorApi && error.estado === 404) && intentos < 2,
  })

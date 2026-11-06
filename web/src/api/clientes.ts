import { useQuery } from '@tanstack/react-query'
import { api } from './cliente'

export type Cliente = {
  id: string
  nombre: string
  empresa: string | null
  email: string | null
  telefono: string | null
  creadoEn: string
  archivadoEn: string | null
}

export type DatosCliente = { nombre: string; empresa: string; email: string; telefono: string }

export const useClientes = (archivados = false) =>
  useQuery({
    queryKey: ['clientes', { archivados }],
    queryFn: () => api<Cliente[]>(`/clientes?archivados=${archivados}`),
  })

export const useCliente = (id: string | undefined) =>
  useQuery({
    queryKey: ['clientes', id],
    queryFn: () => api<Cliente>(`/clientes/${id}`),
    enabled: !!id,
  })

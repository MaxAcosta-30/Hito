import { useQueryClient, type QueryKey } from '@tanstack/react-query'

type Consulta = { queryKey: QueryKey; queryFn?: unknown }

// Pide los datos de la siguiente pantalla en cuanto el cursor (o el foco, o el dedo) llega al enlace:
// cuando llega el clic, la respuesta ya está en camino o en caché.
export function usePrecarga() {
  const cliente = useQueryClient()
  return (consulta: Consulta) => {
    const precargar = () => void cliente.prefetchQuery(consulta as Parameters<typeof cliente.prefetchQuery>[0])
    return { onMouseEnter: precargar, onFocus: precargar, onTouchStart: precargar }
  }
}

/* eslint-disable react/only-export-components -- fábrica de pantallas: Fast Refresh no aplica y no hace falta aquí */
import { lazy, type ComponentType } from 'react'

// Pantalla bajo demanda que se puede precargar. Con React.lazy solo, la primera visita "suspende" aunque
// el código ya esté descargado, y React muestra el esqueleto al menos 300 ms. Una vez precargada,
// se dibuja directamente: sin esqueleto y sin esa espera.
function bajoDemanda<P extends object>(cargar: () => Promise<ComponentType<P>>) {
  let resuelta: ComponentType<P> | null = null
  const traer = () =>
    cargar().then((c) => {
      resuelta = c
      return { default: c }
    })
  const Perezosa = lazy(traer)
  function Pantalla(props: P) {
    const Resuelta = resuelta
    return Resuelta ? <Resuelta {...props} /> : <Perezosa {...props} />
  }
  Pantalla.precargar = () => void traer()
  return Pantalla
}

export const Cobros = bajoDemanda(() => import('./Cobros').then((m) => m.Cobros))
export const Proyectos = bajoDemanda(() => import('./Proyectos').then((m) => m.Proyectos))
export const ProyectoDetalle = bajoDemanda(() => import('./ProyectoDetalle').then((m) => m.ProyectoDetalle))
export const ProyectoFormulario = bajoDemanda(() => import('./ProyectoFormulario').then((m) => m.ProyectoFormulario))
export const Clientes = bajoDemanda(() => import('./Clientes').then((m) => m.Clientes))
export const ClienteFormulario = bajoDemanda(() => import('./ClienteFormulario').then((m) => m.ClienteFormulario))
export const VistaCliente = bajoDemanda(() => import('./VistaCliente').then((m) => m.VistaCliente))

// Se precarga todo cuando el navegador queda libre: la navegación ya no espera el código.
export function precargarPantallas() {
  const precargar = () => [Cobros, Proyectos, ProyectoDetalle, ProyectoFormulario, Clientes, ClienteFormulario, VistaCliente].forEach((p) => p.precargar())
  if ('requestIdleCallback' in window) requestIdleCallback(precargar, { timeout: 2000 })
  else setTimeout(precargar, 1200)
}

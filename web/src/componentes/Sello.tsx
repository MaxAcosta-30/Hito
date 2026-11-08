import { useState } from 'react'
import type { EstadoHito, EstadoProyecto } from '../api/proyectos'

const sellos: Record<EstadoHito, { texto: string; clase: string }> = {
  Pendiente: { texto: 'PENDIENTE', clase: 'pendiente' },
  Entregado: { texto: 'ENTREGADO', clase: 'entregado' },
  CambiosSolicitados: { texto: 'CAMBIOS', clase: 'cambios' },
  Aprobado: { texto: 'APROBADO', clase: 'aprobado' },
  Pagado: { texto: 'PAGADO', clase: 'pagado' },
}

// El estado del hito como sello: siempre con texto, nunca solo color.
export function Sello({ estado, vencido = false }: { estado: EstadoHito; vencido?: boolean }) {
  const sello = sellos[estado]
  // Al cargar no se anima (sería ruido); solo cuando el estado cambia frente a la persona: se estampa.
  const [anterior, setAnterior] = useState(estado)
  const [recien, setRecien] = useState(false)
  if (anterior !== estado) {
    setAnterior(estado)
    setRecien(true)
  }
  return (
    <span className="sellos">
      {vencido && <span className="sello vencido">VENCIDO</span>}
      <span key={estado} className={`sello ${sello.clase}${recien ? ' recien' : ''}`}>
        {sello.texto}
      </span>
    </span>
  )
}

// El estado del proyecto con la misma gramática: punteado = por empezar, discontinuo = en manos del
// cliente, contorno = alcance comprometido, relleno = terminado; cancelado va tachado.
const sellosProyecto: Record<EstadoProyecto, { texto: string; clase: string }> = {
  Borrador: { texto: 'BORRADOR', clase: 'pendiente' },
  Enviado: { texto: 'ENVIADO', clase: 'entregado' },
  Aceptado: { texto: 'ACEPTADO', clase: 'aceptado' },
  Completado: { texto: 'COMPLETADO', clase: 'pagado' },
  Cancelado: { texto: 'CANCELADO', clase: 'cancelado' },
}

export function SelloProyecto({ estado }: { estado: EstadoProyecto }) {
  const sello = sellosProyecto[estado]
  return <span className={`sello ${sello.clase}`}>{sello.texto}</span>
}

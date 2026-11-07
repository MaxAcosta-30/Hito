import { useState } from 'react'
import type { EstadoHito } from '../api/proyectos'

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

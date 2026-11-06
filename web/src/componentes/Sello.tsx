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
  return (
    <span className="sellos">
      {vencido && <span className="sello vencido">VENCIDO</span>}
      <span className={`sello ${sello.clase}`}>{sello.texto}</span>
    </span>
  )
}

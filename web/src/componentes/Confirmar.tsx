import type { ReactNode } from 'react'
import { Dialogo } from './Dialogo'

type Props = {
  titulo: string
  children: ReactNode
  textoConfirmar: string
  // Destructiva: el botón de confirmar va en rojo. Irreversible o deja a alguien sin acceso.
  peligro?: boolean
  abierto: boolean
  pendiente?: boolean
  onConfirmar: () => void
  onCerrar: () => void
}

// Confirmación propia en lugar de window.confirm: dice qué se pierde y el foco inicial está en "Volver",
// así un Enter accidental no ejecuta la acción.
export function Confirmar({ titulo, children, textoConfirmar, peligro = false, abierto, pendiente = false, onConfirmar, onCerrar }: Props) {
  return (
    <Dialogo titulo={titulo} abierto={abierto} onCerrar={onCerrar}>
      <div className="meta">{children}</div>
      <div className="botonera columna">
        <button className={`btn ${peligro ? 'peligro' : 'primario'} ancho`} onClick={onConfirmar} disabled={pendiente}>
          {textoConfirmar}
        </button>
        <button className="btn secundario ancho" data-foco-inicial onClick={onCerrar}>
          Volver
        </button>
      </div>
    </Dialogo>
  )
}

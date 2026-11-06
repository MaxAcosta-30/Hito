import { useEffect, useId, useRef, type ReactNode } from 'react'

// <dialog> nativo con showModal: atrapa el foco, cierra con Escape y vuelve el foco al botón que lo abrió.
export function Dialogo({ titulo, abierto, onCerrar, children }: { titulo: string; abierto: boolean; onCerrar: () => void; children: ReactNode }) {
  const ref = useRef<HTMLDialogElement>(null)
  // Un id por diálogo: hay varios en la misma página y el título debe ser el de este.
  const idTitulo = useId()

  useEffect(() => {
    const dialogo = ref.current
    if (!dialogo) return
    if (abierto && !dialogo.open) dialogo.showModal()
    if (!abierto && dialogo.open) dialogo.close()
  }, [abierto])

  return (
    <dialog ref={ref} className="dialogo" aria-labelledby={idTitulo} onClose={onCerrar}>
      <h3 id={idTitulo}>{titulo}</h3>
      {abierto && children}
    </dialog>
  )
}

import { useEffect, useRef, type ReactNode } from 'react'

// <dialog> nativo con showModal: atrapa el foco, cierra con Escape y vuelve el foco al botón que lo abrió.
export function Dialogo({ titulo, abierto, onCerrar, children }: { titulo: string; abierto: boolean; onCerrar: () => void; children: ReactNode }) {
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialogo = ref.current
    if (!dialogo) return
    if (abierto && !dialogo.open) dialogo.showModal()
    if (!abierto && dialogo.open) dialogo.close()
  }, [abierto])

  return (
    <dialog ref={ref} className="dialogo" aria-labelledby="dialogo-titulo" onClose={onCerrar}>
      <h3 id="dialogo-titulo">{titulo}</h3>
      {abierto && children}
    </dialog>
  )
}

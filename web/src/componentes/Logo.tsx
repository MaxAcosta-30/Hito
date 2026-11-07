// "Hito" es un mojón: la piedra que marca un tramo del camino. El logo es ese mojón asentado sobre doble raya contable.
export function Mojon({ tamano = 28, className }: { tamano?: number; className?: string }) {
  return (
    <svg className={className} width={tamano * 0.8} height={tamano} viewBox="0 0 44 56" aria-hidden="true" focusable="false">
      <path d="M8 50V20a14 14 0 0 1 28 0v30z" fill="var(--firma)" />
      <path d="M4 50.5h36M4 54.5h36" stroke="var(--tinta)" strokeWidth="2" />
    </svg>
  )
}

// Marca completa: mojón + palabra. El texto "Hito" queda para lectores de pantalla y buscadores.
export function Logo({ grande = false }: { grande?: boolean }) {
  return (
    <span className={grande ? 'logo grande' : 'logo'}>
      <Mojon tamano={grande ? 44 : 26} />
      <span className="logo-texto" translate="no">
        HITO
      </span>
    </span>
  )
}

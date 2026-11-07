// "H contable": una H cuyo travesaño es la doble raya de los totales del libro mayor. Se lee como H incluso a 16 px.
export function Simbolo({ tamano = 26, className }: { tamano?: number; className?: string }) {
  return (
    <svg className={className} width={tamano} height={tamano} viewBox="0 0 48 48" aria-hidden="true" focusable="false">
      <rect width="48" height="48" rx="8" fill="var(--firma)" />
      <path d="M14 10v28M34 10v28" stroke="var(--sobre-firma)" strokeWidth="6" />
      <path d="M14 21h20M14 27h20" stroke="var(--sobre-firma)" strokeWidth="3" />
    </svg>
  )
}

// Marca completa: símbolo + palabra. El texto "Hito" queda para lectores de pantalla y buscadores.
export function Logo({ grande = false }: { grande?: boolean }) {
  return (
    <span className={grande ? 'logo grande' : 'logo'}>
      <Simbolo tamano={grande ? 44 : 26} />
      <span className="logo-texto" translate="no">
        HITO
      </span>
    </span>
  )
}

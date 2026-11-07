// Filtro de tinta: los sellos se ven estampados con goma (borde irregular y tinta que no cubre parejo),
// no dibujados. Se monta una vez; los sellos grandes usan #tinta y los chicos #tinta-fina.
export function TintaDefs() {
  return (
    <svg className="defs-svg" width="0" height="0" aria-hidden="true" focusable="false">
      <filter id="tinta" x="-4%" y="-8%" width="108%" height="116%" colorInterpolationFilters="sRGB">
        <feTurbulence type="fractalNoise" baseFrequency="1.1" numOctaves="2" seed="11" result="grano" />
        <feDisplacementMap in="SourceGraphic" in2="grano" scale="1.8" xChannelSelector="R" yChannelSelector="G" result="borde" />
        <feTurbulence type="fractalNoise" baseFrequency="0.55" numOctaves="1" seed="4" result="manchas" />
        <feColorMatrix in="manchas" type="matrix" values="0 0 0 0 0  0 0 0 0 0  0 0 0 0 0  0 0 0 -2.4 2.15" result="cobertura" />
        <feComposite in="borde" in2="cobertura" operator="in" />
      </filter>
      {/* Fina: solo el borde irregular, sin manchas. Para los sellos chicos de la app, que deben leerse siempre. */}
      <filter id="tinta-fina" x="-4%" y="-8%" width="108%" height="116%" colorInterpolationFilters="sRGB">
        <feTurbulence type="fractalNoise" baseFrequency="1.3" numOctaves="1" seed="5" result="grano" />
        <feDisplacementMap in="SourceGraphic" in2="grano" scale="0.9" xChannelSelector="R" yChannelSelector="G" />
      </filter>
    </svg>
  )
}

// Elemento firma de la landing: un libro mayor donde cada hito se asienta y el saldo se recalcula.
// Solo SVG y CSS (transform/opacity); con prefers-reduced-motion queda fijo y completo.
const renglones = [
  { fecha: '07 OCT', concepto: 'Diseño de marca', cargo: '8,000', saldo: '8,000', sello: 'PAGADO' },
  { fecha: '01 NOV', concepto: 'Landing page', cargo: '12,000', saldo: '20,000', sello: 'APROBADO' },
  { fecha: '05 NOV', concepto: 'Tienda en línea', cargo: '20,000', saldo: '40,000', sello: null },
  { fecha: '01 DIC', concepto: 'Capacitación', cargo: '4,000', saldo: '44,000', sello: null },
]

export function LibroAnimado() {
  return (
    <svg className="libro" viewBox="0 0 520 420" role="img" aria-label="Libro de cobros: cuatro hitos asentados, uno pagado y uno aprobado; te deben 36,000 pesos.">
      <g className="libro-encabezado">
        <text x="20" y="40">FECHA</text>
        <text x="96" y="40">CONCEPTO</text>
        <text x="420" y="40" textAnchor="end">CARGO</text>
        <text x="500" y="40" textAnchor="end">SALDO</text>
      </g>
      <line x1="20" x2="500" y1="52" y2="52" className="libro-raya" />
      {renglones.map((r, i) => (
        <g key={r.concepto} className="libro-renglon" style={{ animationDelay: `${0.3 + i * 0.45}s` }}>
          <text x="20" y={100 + i * 60} className="libro-fecha">{r.fecha}</text>
          <text x="96" y={100 + i * 60} className="libro-concepto">{r.concepto}</text>
          <text x="420" y={100 + i * 60} textAnchor="end" className="libro-cifra">{r.cargo}</text>
          <text x="500" y={100 + i * 60} textAnchor="end" className="libro-cifra">{r.saldo}</text>
          <line x1="20" x2="500" y1={118 + i * 60} y2={118 + i * 60} className="libro-renglon-linea" />
        </g>
      ))}
      {renglones.map((r, i) =>
        r.sello ? (
          <g key={r.sello} className="libro-sello" style={{ animationDelay: `${2.3 + i * 0.5}s` }}>
            <rect x="256" y={84 + i * 60} width={r.sello === 'PAGADO' ? 72 : 88} height="22" className={r.sello === 'PAGADO' ? 'relleno-pagado' : 'relleno-aprobado'} />
            <text x={r.sello === 'PAGADO' ? 292 : 300} y={99 + i * 60} textAnchor="middle" className={r.sello === 'PAGADO' ? 'texto-pagado' : 'texto-aprobado'}>
              {r.sello}
            </text>
          </g>
        ) : null,
      )}
      <line x1="20" x2="500" y1="330" y2="330" className="libro-raya libro-total" />
      <line x1="20" x2="500" y1="336" y2="336" className="libro-raya libro-total" />
      <g className="libro-saldo">
        <text x="20" y="392" className="libro-te-deben">TE DEBEN</text>
        <text x="500" y="394" textAnchor="end" className="libro-monto">36,000</text>
      </g>
    </svg>
  )
}

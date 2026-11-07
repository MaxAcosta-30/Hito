import { useState } from 'react'
import type { MesDeCobro } from '../api/cobros'
import { dinero } from '../formato'

// Acordado vs. cobrado, acumulados por mes, como líneas escalonadas: el saldo cambia de golpe al vencer o al pagar.
// Un solo eje (MXN). Leyenda + etiquetas directas al final; tooltip por mes; tabla equivalente para lectores de pantalla.

const ANCHO = 640
const ALTO = 220
const M = { arriba: 16, derecha: 120, abajo: 28, izquierda: 56 }
const corto = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN', notation: 'compact', maximumFractionDigits: 1 })
const nombreMes = new Intl.DateTimeFormat('es-MX', { month: 'short' })
const mesLargo = new Intl.DateTimeFormat('es-MX', { month: 'long', year: 'numeric' })
const aFecha = (iso: string) => {
  const [a, m] = iso.split('-').map(Number)
  return new Date(a, m - 1, 1)
}

// Tope "redondo" del eje: 1, 2 o 5 por potencia de 10.
function tope(maximo: number) {
  if (maximo <= 0) return 1000
  const potencia = 10 ** Math.floor(Math.log10(maximo))
  return [1, 2, 5, 10].map((f) => f * potencia).find((v) => v >= maximo)!
}

export function GraficaCobros({ historial }: { historial: MesDeCobro[] }) {
  const [activo, setActivo] = useState<number | null>(null)
  const y0 = tope(Math.max(...historial.flatMap((m) => [m.acordado, m.cobrado])))
  const anchoPlot = ANCHO - M.izquierda - M.derecha
  const altoPlot = ALTO - M.arriba - M.abajo
  const paso = anchoPlot / historial.length
  const x = (i: number) => M.izquierda + i * paso
  const y = (v: number) => M.arriba + altoPlot - (v / y0) * altoPlot

  // Escalón: horizontal durante el mes, vertical al cambiar de mes.
  const escalon = (valores: number[]) =>
    `M${x(0)} ${y(valores[0])} ` + valores.map((v, i) => `${i > 0 ? `V${y(v)} ` : ''}H${x(i + 1)}`).join(' ')

  const acordado = historial.map((m) => m.acordado)
  const cobrado = historial.map((m) => m.cobrado)
  const ultimo = historial[historial.length - 1]
  const lineas = [0, 0.5, 1].map((f) => f * y0)

  return (
    <figure className="grafica">
      <div className="leyenda" aria-hidden="true">
        <span><i className="serie-acordado" /> Acordado que ya vencía</span>
        <span><i className="serie-cobrado" /> Cobrado</span>
      </div>
      <div className="lienzo-grafica" onPointerLeave={() => setActivo(null)}>
        <svg viewBox={`0 0 ${ANCHO} ${ALTO}`} role="img" aria-label={`Acordado y cobrado acumulados en los últimos ${historial.length} meses. Al día de hoy: acordado ${dinero(ultimo.acordado)}, cobrado ${dinero(ultimo.cobrado)}.`}>
          {lineas.map((v) => (
            <g key={v}>
              <line x1={M.izquierda} x2={ANCHO - M.derecha} y1={y(v)} y2={y(v)} className="cuadricula" />
              <text x={M.izquierda - 8} y={y(v) + 4} textAnchor="end" className="eje">
                {corto.format(v)}
              </text>
            </g>
          ))}
          {historial.map((m, i) => (
            <text key={m.mes} x={x(i) + paso / 2} y={ALTO - 8} textAnchor="middle" className="eje">
              {nombreMes.format(aFecha(m.mes)).replace('.', '')}
            </text>
          ))}
          {activo !== null && <line x1={x(activo) + paso / 2} x2={x(activo) + paso / 2} y1={M.arriba} y2={M.arriba + altoPlot} className="mira" />}
          <path d={escalon(acordado)} className="linea serie-acordado" />
          <path d={escalon(cobrado)} className="linea serie-cobrado" />
          {/* Etiquetas directas al final de cada línea; si se enciman, la de cobrado baja. */}
          {(() => {
            const ya = y(ultimo.acordado)
            let yc = y(ultimo.cobrado)
            if (Math.abs(yc - ya) < 30) yc = ya + 30
            return (
              <>
                <text x={ANCHO - M.derecha + 8} y={ya - 2} className="etiqueta">Acordado</text>
                <text x={ANCHO - M.derecha + 8} y={ya + 13} className="etiqueta-valor">{corto.format(ultimo.acordado)}</text>
                <text x={ANCHO - M.derecha + 8} y={yc - 2} className="etiqueta">Cobrado</text>
                <text x={ANCHO - M.derecha + 8} y={yc + 13} className="etiqueta-valor">{corto.format(ultimo.cobrado)}</text>
              </>
            )
          })()}
          {/* Áreas de toque más grandes que la marca: una columna por mes. */}
          {historial.map((m, i) => (
            <rect key={m.mes} x={x(i)} y={M.arriba} width={paso} height={altoPlot} fill="transparent" onPointerEnter={() => setActivo(i)} onPointerDown={() => setActivo(i)} />
          ))}
        </svg>
        {activo !== null && (
          <div className="tooltip-grafica" style={{ left: `${((x(activo) + paso / 2) / ANCHO) * 100}%` }} aria-hidden="true">
            <b>{mesLargo.format(aFecha(historial[activo].mes))}</b>
            <span><i className="serie-acordado" />Acordado <em>{dinero(historial[activo].acordado)}</em></span>
            <span><i className="serie-cobrado" />Cobrado <em>{dinero(historial[activo].cobrado)}</em></span>
            <span>Diferencia <em>{dinero(historial[activo].acordado - historial[activo].cobrado)}</em></span>
          </div>
        )}
      </div>
      <details className="tabla-grafica">
        <summary>Ver como tabla</summary>
        <table>
          <thead>
            <tr><th>Mes</th><th className="num">Acordado</th><th className="num">Cobrado</th></tr>
          </thead>
          <tbody>
            {historial.map((m) => (
              <tr key={m.mes}><td>{mesLargo.format(aFecha(m.mes))}</td><td className="num">{dinero(m.acordado)}</td><td className="num">{dinero(m.cobrado)}</td></tr>
            ))}
          </tbody>
        </table>
      </details>
    </figure>
  )
}

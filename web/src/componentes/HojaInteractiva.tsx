import { useState } from 'react'
import { dinero } from '../formato'
import { useCifraAnimada } from './useCifraAnimada'

// La hoja que sale del expediente en la landing: un ejemplo que se puede operar.
// Aprobar estampa APROBADO; registrar el pago estampa PAGADO y baja lo que te deben.
// Cada sello es un asiento nuevo: el folio corre, como en el historial real de Hito.
type Paso = 'entregado' | 'aprobado' | 'pagado'

const avisos: Record<Paso, string> = {
  entregado: 'La landing está entregada. Apruébala como lo haría tu cliente.',
  aprobado: 'Aprobada y sellada. Ahora cóbrala.',
  pagado: 'Pago asentado. Ya te deben $12,000 menos.',
}

const asientos: Record<Paso, number> = { entregado: 7, aprobado: 8, pagado: 9 }

export function HojaInteractiva() {
  const [paso, setPaso] = useState<Paso>('entregado')
  const teDeben = useCifraAnimada(paso === 'pagado' ? 24000 : 36000)

  return (
    // La clase del paso reinicia el "golpe" de la hoja en cada sello.
    <div className={`hoja-expediente golpe-${paso}`}>
      <svg className="clip" viewBox="0 0 28 64" aria-hidden="true" focusable="false">
        <path d="M9 60V12a5 5 0 0 1 10 0v40a8 8 0 0 1-16 0V16" fill="none" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
      </svg>
      <div className="hoja-encabezado">
        <h2>Tienda en línea</h2>
        <span className="marca-ejemplo">Ejemplo</span>
        <span>Panadería Rosales · versión 1 aceptada</span>
        <span className="folio" aria-label={`Folio 0042, asiento ${asientos[paso]}`}>
          Folio 0042 · asiento <b key={paso}>{asientos[paso]}</b>
        </span>
      </div>
      <ol className="hoja-renglones">
        <li>
          <span className="concepto">Diseño de marca</span>
          <span className="importe">8,000</span>
          <span className="sello pagado">PAGADO</span>
        </li>
        <li className="renglon-activo">
          <span className="concepto">Landing page</span>
          <span className="importe">12,000</span>
          {paso === 'entregado' && <span className="sello entregado">ENTREGADO</span>}
          {paso === 'aprobado' && <span className="sello aprobado">APROBADO</span>}
          {paso === 'pagado' && <span className="sello pagado">PAGADO</span>}
          {/* El sello grande que cae sobre el renglón; la etiqueta chica de la columna es el estado accesible. */}
          {paso !== 'entregado' && (
            <span key={paso} className={`sello-golpe ${paso}`} aria-hidden="true">
              {paso === 'aprobado' ? 'APROBADO' : 'PAGADO'}
            </span>
          )}
        </li>
        <li>
          <span className="concepto">Tienda en línea</span>
          <span className="importe">20,000</span>
          <span className="sello entregado">ENTREGADO</span>
        </li>
        <li>
          <span className="concepto">Capacitación</span>
          <span className="importe">4,000</span>
          <span className="sello pendiente">PENDIENTE</span>
        </li>
      </ol>
      <div className="hoja-total">
        <span>Te deben</span>
        <strong aria-hidden="true">{dinero(Math.round(teDeben))}</strong>
        <span className="solo-lectores">{dinero(paso === 'pagado' ? 24000 : 36000)}</span>
      </div>
      <div className="hoja-accion">
        <p role="status">{avisos[paso]}</p>
        {paso === 'entregado' && (
          <button className="btn primario" onClick={() => setPaso('aprobado')}>
            Aprobar entrega
          </button>
        )}
        {paso === 'aprobado' && (
          <button className="btn primario" onClick={() => setPaso('pagado')}>
            Registrar pago de $12,000
          </button>
        )}
        {paso === 'pagado' && (
          <button className="btn contorno" onClick={() => setPaso('entregado')}>
            Otra vez
          </button>
        )}
      </div>
    </div>
  )
}

import { mensajeDeError } from '../api/errores'

// Carga: renglones vacíos, sin spinners.
export function Esqueleto({ renglones = 4 }: { renglones?: number }) {
  return (
    <div className="hoja" role="status">
      <span className="solo-lectores">Cargando…</span>
      {Array.from({ length: renglones }, (_, i) => (
        <div key={i} className={i % 2 ? 'esqueleto corto' : 'esqueleto'} />
      ))}
    </div>
  )
}

// Error: dice qué pasó y ofrece reintentar.
export function ErrorCarga({ error, onReintentar }: { error: unknown; onReintentar: () => void }) {
  return (
    <div className="aviso aviso-error" role="alert">
      <div>
        <b>{mensajeDeError(error)}</b>
        <div className="meta">Revisa tu conexión e inténtalo de nuevo. Tus datos están a salvo.</div>
        <button className="btn secundario" onClick={onReintentar}>
          Reintentar
        </button>
      </div>
    </div>
  )
}

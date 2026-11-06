type Props = { estado: 'despertando' | 'error'; onReintentar: () => void }

export function Despertando({ estado, onReintentar }: Props) {
  return (
    <main className="contenido angosto">
      <h1 className="marca-grande">Hito</h1>
      {estado === 'despertando' ? (
        <div className="aviso" role="status">
          <span className="punto" aria-hidden="true" />
          <div>
            <b>Despertando la demo, puede tardar hasta un minuto</b>
            <div className="meta">El servidor gratuito se apaga cuando nadie lo usa.</div>
          </div>
        </div>
      ) : (
        <div className="aviso aviso-error" role="alert">
          <div>
            <b>No pudimos conectar con el servidor</b>
            <div className="meta">Revisa tu conexión e inténtalo de nuevo.</div>
            <button className="btn secundario" onClick={onReintentar}>
              Reintentar
            </button>
          </div>
        </div>
      )}
    </main>
  )
}

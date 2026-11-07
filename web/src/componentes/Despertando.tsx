import { useEffect, useState } from 'react'

type Props = { estado: 'despertando' | 'error'; onReintentar: () => void }

// Mensajes según el tiempo de espera. No hay un progreso real que medir, así que no se inventa un porcentaje.
const fases = [
  { desde: 0, texto: 'El servidor gratuito se apaga cuando nadie lo usa.' },
  { desde: 10, texto: 'Encendiendo el servidor…' },
  { desde: 30, texto: 'Ya casi: la API está terminando de arrancar.' },
  { desde: 60, texto: 'Está tardando más de lo normal. Seguimos intentando.' },
]

// Si la API responde rápido no se muestra nada: evita un destello de esta pantalla en cada visita.
const RETRASO_MS = 400

export function Despertando({ estado, onReintentar }: Props) {
  const [visible, setVisible] = useState(false)
  const [segundos, setSegundos] = useState(0)

  useEffect(() => {
    const mostrar = setTimeout(() => setVisible(true), RETRASO_MS)
    const reloj = setInterval(() => setSegundos((s) => s + 1), 1000)
    return () => {
      clearTimeout(mostrar)
      clearInterval(reloj)
    }
  }, [])

  if (estado === 'error') {
    return (
      <main className="contenido angosto">
        <h1 className="marca-grande">Hito</h1>
        <div className="aviso aviso-error" role="alert">
          <div>
            <b>No pudimos conectar con el servidor</b>
            <div className="meta">Revisa tu conexión e inténtalo de nuevo.</div>
            <button className="btn secundario" onClick={onReintentar}>
              Reintentar
            </button>
          </div>
        </div>
      </main>
    )
  }

  if (!visible) return null

  const fase = fases.findLast((f) => segundos >= f.desde)!
  return (
    <main className="contenido angosto despertando">
      <h1 className="solo-lectores">Hito</h1>
      {/* El sello de la marca: se estampa una vez y luego respira mientras se espera. */}
      <span className="sello grande sello-marca" aria-hidden="true">
        HITO
      </span>
      <div role="status">
        <p className="titular">Despertando la demo, puede tardar hasta un minuto</p>
        <p className="meta">{fase.texto}</p>
      </div>
      {/* Fuera de la región viva: el lector de pantalla no anuncia cada segundo. */}
      <p className="contador" aria-hidden="true">
        {segundos} s
      </p>
    </main>
  )
}

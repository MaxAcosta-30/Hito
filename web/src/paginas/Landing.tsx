import { useEffect } from 'react'
import { Link } from 'react-router'
import { esperarApi } from '../api/esperarApi'
import { HojaInteractiva } from '../componentes/HojaInteractiva'
import { IconoSalida } from '../componentes/Iconos'
import { Logo } from '../componentes/Logo'

// Así se arma: una sola hoja; cada renglón recibe su sello, en orden, como en un expediente real.
const pasos = [
  {
    paso: 'Acuerda',
    texto: 'Escribe los hitos con monto y fecha. Tu cliente los acepta desde un enlace, sin crear cuenta, y quedan sellados. ¿Cambió algo? Va una versión nueva.',
    sello: { texto: 'ACEPTADO', clase: 'aceptado' },
  },
  {
    paso: 'Entrega',
    texto: 'Marca cada hito como entregado con el enlace al trabajo. Tu cliente lo aprueba o pide cambios, y tiene que decir cuáles. Adiós al «ahí lo vemos».',
    sello: { texto: 'APROBADO', clase: 'aprobado' },
  },
  {
    paso: 'Cobra',
    texto: 'Registra cada pago. El panel te dice cuánto te deben, qué ya está aprobado y qué se venció. Sin hojas de cálculo ni mensajes de «¿ya viste mi factura?».',
    sello: { texto: 'PAGADO', clase: 'pagado' },
  },
]

// Los tres vienen en la demo: quien lee puede abrirlos y verlos funcionar.
const casos = [
  {
    cliente: 'Panadería Rosales',
    quien: 'Diseñadora web',
    proyecto: 'Tienda en línea',
    texto: 'Cuatro hitos por $44,000. Su cliente aprueba la landing desde WhatsApp y ella ve al instante que le deben $36,000.',
    sellos: [
      ['PAGADO', 'pagado'],
      ['APROBADO', 'aprobado'],
    ],
  },
  {
    cliente: 'Clínica Norte',
    quien: 'Desarrollador de apps',
    proyecto: 'App de citas',
    texto: 'La clínica pide cambios al prototipo con un comentario. Él corrige, entrega de nuevo y todo queda en el historial.',
    sellos: [['CAMBIOS', 'cambios']],
  },
  {
    cliente: 'Estudio Vega',
    quien: 'Estudio de arquitectura',
    proyecto: 'Sitio institucional',
    texto: 'Arma el alcance en borrador y lo ajusta antes de enviarlo. Cuando el cliente lo acepte, hitos y montos no se mueven.',
    sellos: [['PENDIENTE', 'pendiente']],
  },
]

// Página pública: no espera a la API para mostrarse. Mientras se lee, la despierta en segundo plano
// (Render Free se duerme) para que "Probar la demo" responda rápido.
export function Landing() {
  useEffect(() => {
    const control = new AbortController()
    esperarApi('/api/health/live', control.signal)
    return () => control.abort()
  }, [])

  return (
    <div className="exp">
      <a className="saltar" href="#principal">
        Saltar al contenido
      </a>
      <header className="exp-escritorio">
        <Link to="/" className="logo-enlace" aria-label="Hito, inicio">
          <Logo />
        </Link>
        <Link className="exp-entrar" to="/entrar">
          Entrar
        </Link>
      </header>

      <main id="principal">
        <section className="carpeta" aria-labelledby="titulo">
          <nav className="carpeta-pestanas" aria-label="Secciones">
            <a href="#principal" aria-current="page">
              Expediente
            </a>
            <a href="#como">Cómo se arma</a>
            <a href="#casos">Casos</a>
            <a href="#autor">Quién lo hizo</a>
          </nav>
          <div className="carpeta-cuerpo">
            <div className="carpeta-etiqueta">
              <h1 id="titulo">
                Lo que acordaste, <span>asentado.</span>
              </h1>
              <p className="carpeta-bajada">Hitos, entregas y pagos en un solo expediente: lo acordado se sella y lo que te deben se ve de un vistazo.</p>
              <div className="carpeta-botones">
                <Link className="btn tinta grande" to="/demo">
                  Probar la demo<span className="solo-ancho"> · sin registro</span>
                </Link>
                <Link className="enlace-crear" to="/registro">
                  o crea tu cuenta
                </Link>
              </div>
            </div>
            <HojaInteractiva />
            <p className="carpeta-nota">Un clic y tienes un expediente de ejemplo. Se borra solo en 24 horas, sin trámites.</p>
          </div>
        </section>

        <section id="como" className="formatos" aria-labelledby="como-titulo">
          <h2 id="como-titulo">Así se arma el expediente</h2>
          <ol className="formatos-hoja">
            {pasos.map((p) => (
              <li key={p.paso} className="formato">
                <h3>{p.paso}</h3>
                <p>{p.texto}</p>
                <span className={`sello grande ${p.sello.clase}`}>{p.sello.texto}</span>
              </li>
            ))}
          </ol>
        </section>

        <section id="casos" className="archivo" aria-labelledby="casos-titulo">
          <div className="archivo-texto">
            <h2 id="casos-titulo">Expedientes de ejemplo</h2>
            <p className="archivo-bajada">Los tres vienen en la demo. Ábrelos, sella, cobra: nadie se va a enojar.</p>
            <Link className="btn primario grande" to="/demo">
              Abrir la demo con estos casos
            </Link>
          </div>
          <ul className="archivo-abanico">
            {casos.map((c) => (
              <li key={c.cliente} className="archivo-carpeta">
                <span className="archivo-pestana">{c.cliente}</span>
                <div className="archivo-cuerpo">
                  <h3>{c.proyecto}</h3>
                  <span className="archivo-quien">{c.quien}</span>
                  <p>{c.texto}</p>
                  <span className="archivo-sellos">
                    {c.sellos.map(([texto, clase]) => (
                      <span key={texto} className={`sello ${clase}`}>
                        {texto}
                      </span>
                    ))}
                  </span>
                </div>
              </li>
            ))}
          </ul>
        </section>

        <section className="cliente-seccion" aria-labelledby="cliente-titulo">
          <div className="cliente-hoja">
            <h2 id="cliente-titulo">Tu cliente aprueba desde su celular.</h2>
            <p>Le mandas el enlace por WhatsApp, revisa el alcance y aprueba cada entrega con un toque. Sin cuenta ni contraseña que se le olvide. El enlace es privado, vence y lo puedes revocar.</p>
          </div>
          <picture className="cliente-celular">
            <source srcSet="/landing/cliente.webp" type="image/webp" />
            <img src="/landing/cliente.png" width="390" height="844" loading="lazy" decoding="async" alt="Vista del cliente en el celular: hitos con sus sellos y el botón Aprobar entrega." />
          </picture>
        </section>

        <section id="autor" className="firma" aria-labelledby="autor-titulo">
          <h2 id="autor-titulo">Quién lo hizo</h2>
          <p className="firma-bajada">Hito es un proyecto de portafolio hecho para defenderse en una entrevista, pieza por pieza.</p>
          <dl className="firma-acta">
            <div>
              <dt>.NET 10</dt>
              <dd>En capas: las reglas de estado y de dinero viven en el dominio, con pruebas unitarias.</dd>
            </div>
            <div>
              <dt>PostgreSQL</dt>
              <dd>Con EF Core; cada usuario solo ve lo suyo, y hay pruebas de integración que lo comprueban.</dd>
            </div>
            <div>
              <dt>React 19</dt>
              <dd>Con TypeScript y TanStack Query; un e2e con Playwright recorre el cobro completo.</dd>
            </div>
            <div>
              <dt>CI/CD</dt>
              <dd>GitHub Actions con una prueba de despliegue en Kubernetes (kind) antes de migrar.</dd>
            </div>
          </dl>
          <div className="firma-pie">
            <div className="firma-linea">
              <span className="firma-nombre">Max Acosta</span>
              <span>Diseñó y construyó Hito</span>
            </div>
            <a className="btn contorno" href="https://github.com/MaxAcosta-30/Hito" target="_blank" rel="noopener noreferrer">
              Ver el código en GitHub <IconoSalida />
            </a>
          </div>
        </section>
      </main>

      <footer className="exp-pie">
        <Logo />
        <span>Cada entrega aprobada, cada cobro claro.</span>
        <Link to="/privacidad">Aviso de privacidad</Link>
        <span>© 2026 Max Acosta · MIT</span>
      </footer>
    </div>
  )
}

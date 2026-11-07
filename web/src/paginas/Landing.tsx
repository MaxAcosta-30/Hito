import { useEffect } from 'react'
import { Link } from 'react-router'
import { esperarApi } from '../api/esperarApi'
import { LibroAnimado } from '../componentes/LibroAnimado'
import { Logo } from '../componentes/Logo'

// Casos de ejemplo: los tres son proyectos que trae la demo, así quien lee puede abrirlos y verlos funcionar.
const casos = [
  {
    quien: 'Diseñadora web',
    proyecto: 'Tienda en línea · Panadería Rosales',
    texto: 'Cuatro hitos por $44,000. Su cliente aprueba la landing desde WhatsApp y ella ve al instante que le deben $36,000, con $12,000 ya aprobados.',
    sellos: ['PAGADO', 'APROBADO'],
  },
  {
    quien: 'Desarrollador de apps',
    proyecto: 'App de citas · Clínica Norte',
    texto: 'La clínica pide cambios al prototipo con un comentario. Él corrige, entrega de nuevo y todo queda en el historial: quién, qué y cuándo.',
    sellos: ['CAMBIOS'],
  },
  {
    quien: 'Estudio de arquitectura',
    proyecto: 'Sitio institucional · Estudio Vega',
    texto: 'Arma el alcance en borrador y lo ajusta antes de enviarlo. Cuando el cliente lo acepte, hitos y montos quedan congelados.',
    sellos: ['PENDIENTE'],
  },
]

const pasos = [
  { n: '01', titulo: 'Acuerda', texto: 'Escribe los hitos con monto y fecha. Tu cliente los acepta desde un enlace privado, sin crear cuenta, y quedan congelados.' },
  { n: '02', titulo: 'Entrega', texto: 'Marca cada hito como entregado con el enlace al trabajo. Tu cliente lo aprueba o te pide cambios con un comentario.' },
  { n: '03', titulo: 'Cobra', texto: 'Registra cada pago. El panel te dice cuánto te deben, qué ya está aprobado y qué está vencido.' },
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
    <div className="landing">
      <a className="saltar" href="#principal">
        Saltar al contenido
      </a>
      <header className="landing-nav">
        <Link to="/" className="logo-enlace" aria-label="Hito, inicio">
          <Logo />
        </Link>
        <nav aria-label="Principal">
          <a href="#como">Cómo funciona</a>
          <Link to="/entrar">Entrar</Link>
          <Link className="btn primario" to="/demo">
            Probar la demo
          </Link>
        </nav>
      </header>

      <main id="principal">
        <section className="landing-hero">
          <div className="hero-texto">
            <p className="rotulo">Para freelancers en México</p>
            <h1>
              Lo que acordaste, <span>asentado.</span>
            </h1>
            <p className="hero-bajada">Hitos, entregas, aprobaciones y pagos en un solo libro. El saldo se calcula solo y lo vencido salta a la vista.</p>
            <div className="hero-botones">
              <Link className="btn primario grande" to="/demo">
                Probar la demo · sin registro
              </Link>
              <Link className="btn secundario grande" to="/registro">
                Crear cuenta
              </Link>
            </div>
            <p className="hero-nota">Un clic crea una cuenta de ejemplo que se borra en 24 horas.</p>
          </div>
          <div className="hero-libro">
            <LibroAnimado />
          </div>
        </section>

        <section id="como" className="landing-pasos" aria-labelledby="como-titulo">
          <h2 id="como-titulo">Cómo funciona</h2>
          <ol>
            {pasos.map((p) => (
              <li key={p.n}>
                <span className="paso-numero" aria-hidden="true">
                  {p.n}
                </span>
                <h3>{p.titulo}</h3>
                <p>{p.texto}</p>
              </li>
            ))}
          </ol>
        </section>

        <section className="landing-casos" aria-labelledby="casos-titulo">
          <h2 id="casos-titulo">Casos de ejemplo</h2>
          <p className="casos-bajada">Los tres vienen en la demo: ábrela y recórrelos.</p>
          <ul>
            {casos.map((c) => (
              <li key={c.quien}>
                <span className="rotulo">{c.quien}</span>
                <h3>{c.proyecto}</h3>
                <p>{c.texto}</p>
                <span className="casos-sellos">
                  {c.sellos.map((s) => (
                    <span key={s} className={`sello ${{ PAGADO: 'pagado', APROBADO: 'aprobado', CAMBIOS: 'cambios', PENDIENTE: 'pendiente' }[s]}`}>
                      {s}
                    </span>
                  ))}
                </span>
              </li>
            ))}
          </ul>
          <Link className="btn primario" to="/demo">
            Abrir la demo con estos casos
          </Link>
        </section>

        <section className="landing-cliente" aria-labelledby="cliente-titulo">
          <div>
            <p className="rotulo">Del lado de tu cliente</p>
            <h2 id="cliente-titulo">Un enlace. Sin cuenta. Desde su celular.</h2>
            <p>Abre el enlace que le mandaste por WhatsApp, revisa el alcance y aprueba cada entrega con un toque. El enlace es privado, vence y lo puedes revocar.</p>
          </div>
          <img src="/landing/cliente.png" width="390" height="844" loading="lazy" alt="Vista del cliente en el celular: hitos con sus sellos y el botón Aprobar entrega." />
        </section>

        <section className="landing-tecnica" aria-labelledby="tecnica-titulo">
          <h2 id="tecnica-titulo">Hecho para defenderse en una entrevista</h2>
          <ul>
            <li><b>.NET 10</b> en capas: las reglas de estado y de dinero viven en el dominio, con pruebas unitarias.</li>
            <li><b>PostgreSQL</b> con EF Core; cada usuario solo ve lo suyo, con pruebas de integración que lo comprueban.</li>
            <li><b>React 19</b> con TypeScript y TanStack Query; e2e con Playwright del flujo de cobro completo.</li>
            <li><b>CI/CD</b> en GitHub Actions, con una prueba de despliegue en Kubernetes (kind) antes de migrar.</li>
          </ul>
          <a className="btn secundario" href="https://github.com/MaxAcosta-30/Hito" target="_blank" rel="noopener noreferrer">
            Ver el código en GitHub ↗
          </a>
        </section>
      </main>

      <footer className="landing-pie">
        <Logo />
        <span>Cada entrega aprobada, cada cobro claro.</span>
        <Link to="/privacidad">Aviso de privacidad</Link>
        <span>© 2026 Max Acosta · MIT</span>
      </footer>
    </div>
  )
}

import { Suspense, useEffect, useState } from 'react'
import { Navigate, NavLink, Outlet, Route, Routes, useLocation } from 'react-router'
import { esperarApi } from './api/esperarApi'
import { useSesion } from './auth/contexto'
import { SesionProvider } from './auth/SesionProvider'
import { consultaPanel } from './api/cobros'
import { consultaClientes } from './api/clientes'
import { usePrecarga } from './api/precarga'
import { consultaProyectos } from './api/proyectos'
import { Despertando } from './componentes/Despertando'
import { Esqueleto } from './componentes/Estados'
import { Logo } from './componentes/Logo'
import { ToastProvider } from './componentes/ToastProvider'
import { ClienteFormulario, Clientes, Cobros, precargarPantallas, ProyectoDetalle, ProyectoFormulario, Proyectos, VistaCliente } from './paginas/pantallas'
import { Entrar } from './paginas/Entrar'
import { IniciarDemo } from './paginas/IniciarDemo'
import { Landing } from './paginas/Landing'
import { Privacidad } from './paginas/Privacidad'
import { Registro } from './paginas/Registro'


export default function App() {
  const [api, setApi] = useState<'despertando' | 'lista' | 'error'>('despertando')
  const [intento, setIntento] = useState(0)
  const { pathname } = useLocation()

  // Render Free se apaga sin tráfico: antes de todo, esperar a que la API responda.
  useEffect(() => {
    const control = new AbortController()
    esperarApi('/api/health/live', control.signal).then((lista) => {
      if (!control.signal.aborted) setApi(lista ? 'lista' : 'error')
    })
    precargarPantallas()
    return () => control.abort()
  }, [intento])

  // La landing y el aviso de privacidad son texto fijo: no esperan a que despierte la API.
  if (pathname === '/') return <Landing />
  if (pathname === '/privacidad') return <Privacidad />

  if (api !== 'lista') {
    return (
      <Despertando
        key={intento}
        estado={api}
        onReintentar={() => {
          setApi('despertando')
          setIntento((n) => n + 1)
        }}
      />
    )
  }

  // El cliente entra con su enlace y sin cuenta: su vista no carga la sesión del freelancer.
  return (
    <Suspense fallback={<Cargando />}>
    <Routes>
      <Route path="/p/:token" element={<VistaCliente />} />
      <Route path="*" element={<AppFreelancer />} />
    </Routes>
    </Suspense>
  )
}

function AppFreelancer() {
  return (
    <SesionProvider>
      <ToastProvider>
        <Routes>
          <Route element={<SoloAnonimo />}>
            <Route path="/entrar" element={<Entrar />} />
            <Route path="/registro" element={<Registro />} />
          </Route>
          <Route element={<Protegida />}>
            <Route path="/cobros" element={<Cobros />} />
            <Route path="/proyectos" element={<Proyectos />} />
            <Route path="/proyectos/nuevo" element={<ProyectoFormulario />} />
            <Route path="/proyectos/:id" element={<ProyectoDetalle />} />
            <Route path="/proyectos/:id/alcance" element={<ProyectoFormulario />} />
            <Route path="/clientes" element={<Clientes />} />
            <Route path="/clientes/nuevo" element={<ClienteFormulario />} />
            <Route path="/clientes/:id" element={<ClienteFormulario />} />
          </Route>
          <Route path="/demo" element={<IniciarDemo />} />
          <Route path="*" element={<Navigate to="/cobros" replace />} />
        </Routes>
      </ToastProvider>
    </SesionProvider>
  )
}

function Cargando() {
  return (
    <main className="contenido" aria-busy="true">
      <div className="hoja">
        <div className="esqueleto" />
        <div className="esqueleto corto" />
      </div>
    </main>
  )
}

function SoloAnonimo() {
  const { estado } = useSesion()
  if (estado.tipo === 'cargando') return <Cargando />
  return estado.tipo === 'autenticado' ? <Navigate to="/cobros" replace /> : <Outlet />
}

function Protegida() {
  const { estado, salir } = useSesion()
  const ubicacion = useLocation()
  const precargar = usePrecarga()
  if (estado.tipo === 'cargando') return <Cargando />
  if (estado.tipo === 'anonimo') return <Navigate to="/entrar" replace state={{ desde: ubicacion.pathname }} />

  return (
    <>
      <a className="saltar" href="#contenido">
        Saltar al contenido
      </a>
      <header className="barra">
        <Logo />
        <nav aria-label="Principal">
          <NavLink to="/cobros" {...precargar(consultaPanel())}>
            Cobros
          </NavLink>
          <NavLink to="/proyectos" {...precargar(consultaProyectos())}>
            Proyectos
          </NavLink>
          <NavLink to="/clientes" {...precargar(consultaClientes())}>
            Clientes
          </NavLink>
        </nav>
        <button className="btn texto" onClick={salir}>
          Salir
        </button>
      </header>
      {estado.usuario.esDemo && (
        <p className="aviso-demo" role="note">
          Estás en la demo: los datos son de ejemplo y se borran en 24 horas.
        </p>
      )}
      <main id="contenido" key={ubicacion.pathname} className="contenido" tabIndex={-1}>
        {/* La barra se queda; solo el contenido muestra su esqueleto mientras baja la pantalla. */}
        <Suspense fallback={<Esqueleto renglones={5} />}>
          <Outlet />
        </Suspense>
      </main>
    </>
  )
}

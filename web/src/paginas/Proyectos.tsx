import { Link } from 'react-router'
import { useProyectos } from '../api/proyectos'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { dinero, fechaCorta } from '../formato'

export function Proyectos() {
  const proyectos = useProyectos()

  return (
    <>
      <div className="encabezado">
        <h1>Proyectos</h1>
        <Link className="btn primario" to="/proyectos/nuevo">
          Nuevo proyecto
        </Link>
      </div>
      {proyectos.isPending ? (
        <Esqueleto />
      ) : proyectos.isError ? (
        <ErrorCarga error={proyectos.error} onReintentar={() => proyectos.refetch()} />
      ) : proyectos.data.length === 0 ? (
        <div className="hoja vacio">
          <h3>Aún no tienes proyectos</h3>
          <p className="meta">Crea un proyecto con sus hitos y envíaselo a tu cliente para que acepte el alcance.</p>
          <Link className="btn primario" to="/proyectos/nuevo">
            Crear proyecto
          </Link>
        </div>
      ) : (
        <div className="hoja">
          {proyectos.data.map((p) => (
            <Link key={p.id} className="renglon" to={`/proyectos/${p.id}`}>
              <b>{p.nombre}</b>
              <span className="monto">{dinero(p.total)}</span>
              <small>
                {p.cliente.nombre} · {p.estado} · versión {p.version} · {fechaCorta(p.actualizadoEn)}
              </small>
            </Link>
          ))}
        </div>
      )}
    </>
  )
}

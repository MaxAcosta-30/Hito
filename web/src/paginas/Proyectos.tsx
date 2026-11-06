import { Link, useSearchParams } from 'react-router'
import { useProyectos, type EstadoProyecto, type ProyectoResumen } from '../api/proyectos'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { dinero, fechaCorta } from '../formato'

const estados: EstadoProyecto[] = ['Borrador', 'Enviado', 'Aceptado', 'Completado', 'Cancelado']

// Sin acentos ni mayúsculas: "clinica" encuentra "Clínica Norte".
const normalizar = (texto: string) => texto.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase()

// Se filtra en el navegador: el listado ya llega completo y un freelancer tiene decenas de proyectos, no miles.
function filtrar(lista: ProyectoResumen[], texto: string, estado: string) {
  const buscado = normalizar(texto.trim())
  return lista.filter((p) => (!estado || p.estado === estado) && (!buscado || normalizar(`${p.nombre} ${p.cliente.nombre}`).includes(buscado)))
}

export function Proyectos() {
  const proyectos = useProyectos()
  // Búsqueda y estado viven en la URL: al volver del detalle, el filtro sigue ahí.
  const [parametros, setParametros] = useSearchParams()
  const texto = parametros.get('q') ?? ''
  const estado = parametros.get('estado') ?? ''
  const cambiar = (clave: 'q' | 'estado', valor: string) =>
    setParametros(
      (actuales) => {
        const nuevos = new URLSearchParams(actuales)
        if (valor) nuevos.set(clave, valor)
        else nuevos.delete(clave)
        return nuevos
      },
      { replace: true },
    )
  const visibles = proyectos.data ? filtrar(proyectos.data, texto, estado) : []

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
        <>
          <div className="filtros" role="search">
            <div className="campo">
              <label htmlFor="buscar">Buscar</label>
              <input id="buscar" name="q" type="search" autoComplete="off" spellCheck={false} placeholder="Proyecto o cliente…" value={texto} onChange={(e) => cambiar('q', e.target.value)} />
            </div>
            <div className="campo">
              <label htmlFor="estado">Estado</label>
              <select id="estado" name="estado" value={estado} onChange={(e) => cambiar('estado', e.target.value)}>
                <option value="">Todos</option>
                {estados.map((e) => (
                  <option key={e} value={e}>
                    {e}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <p className="meta" role="status">
            {visibles.length === proyectos.data.length ? `${visibles.length} ${visibles.length === 1 ? 'proyecto' : 'proyectos'}` : `${visibles.length} de ${proyectos.data.length} proyectos`}
          </p>
          {visibles.length === 0 ? (
            <div className="hoja vacio">
              <h3>Ningún proyecto coincide</h3>
              <p className="meta">Prueba con otra palabra o con otro estado.</p>
              <button className="btn secundario" onClick={() => setParametros({}, { replace: true })}>
                Quitar filtros
              </button>
            </div>
          ) : (
            <div className="hoja">
              {visibles.map((p) => (
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
      )}
    </>
  )
}

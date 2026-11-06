import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { api } from '../api/cliente'
import { useClientes } from '../api/clientes'
import { erroresDeFormulario } from '../api/errores'
import { useProyecto, versionDe, type HitoFormulario, type Proyecto } from '../api/proyectos'
import { Campo, CampoArea } from '../componentes/Campo'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { useToast } from '../componentes/toast'
import { useEnfocarError } from '../componentes/useEnfocarError'
import { dinero, leerMonto } from '../formato'

type HitoEditable = { clave: string; id?: string; titulo: string; monto: string; fechaEntrega: string; entregable: string }

let siguiente = 0
const vacio = (): HitoEditable => ({ clave: `n${siguiente++}`, titulo: '', monto: '', fechaEntrega: '', entregable: '' })

// /proyectos/nuevo crea el proyecto; /proyectos/:id/alcance edita la versión en borrador.
export function ProyectoFormulario() {
  const { id } = useParams()
  const proyecto = useProyecto(id)
  const clientes = useClientes()

  if ((id && proyecto.isPending) || clientes.isPending) return <Esqueleto renglones={8} />
  if (id && proyecto.isError) return <ErrorCarga error={proyecto.error} onReintentar={() => proyecto.refetch()} />
  if (clientes.isError) return <ErrorCarga error={clientes.error} onReintentar={() => clientes.refetch()} />

  if (proyecto.data && versionDe(proyecto.data, proyecto.data.ultimaVersion).estado !== 'Borrador') {
    return (
      <div className="hoja vacio">
        <h3>Esta versión ya se envió</h3>
        <p className="meta">Para cambiar el alcance crea una nueva versión desde el detalle del proyecto.</p>
        <Link className="btn primario" to={`/proyectos/${proyecto.data.id}`}>
          Volver al proyecto
        </Link>
      </div>
    )
  }

  if (!proyecto.data && clientes.data.length === 0) {
    return (
      <div className="hoja vacio">
        <h3>Primero agrega un cliente</h3>
        <p className="meta">Cada proyecto pertenece a un cliente. Agrégalo y vuelve aquí.</p>
        <Link className="btn primario" to="/clientes/nuevo">
          Agregar un cliente
        </Link>
      </div>
    )
  }

  return <Formulario key={id ?? 'nuevo'} proyecto={proyecto.data} clientes={clientes.data} />
}

function Formulario({ proyecto, clientes }: { proyecto?: Proyecto; clientes: { id: string; nombre: string }[] }) {
  const queryClient = useQueryClient()
  const navegar = useNavigate()
  const toast = useToast()
  const borrador = proyecto && versionDe(proyecto, proyecto.ultimaVersion)
  const [clienteId, setClienteId] = useState(proyecto?.cliente.id ?? (clientes.length === 1 ? clientes[0].id : ''))
  const [nombre, setNombre] = useState(proyecto?.nombre ?? '')
  const [hitos, setHitos] = useState<HitoEditable[]>(
    borrador?.hitos.map((h) => ({ clave: h.id, id: h.id, titulo: h.titulo, monto: h.monto.toFixed(2), fechaEntrega: h.fechaEntrega, entregable: h.entregable })) ?? [vacio()],
  )

  const guardar = useMutation({
    mutationFn: () => {
      const cuerpo = {
        nombre,
        hitos: hitos.map<HitoFormulario>((h) => ({ id: h.id, titulo: h.titulo, monto: leerMonto(h.monto) || 0, fechaEntrega: h.fechaEntrega || null, entregable: h.entregable })),
      }
      return proyecto
        ? api<Proyecto>(`/proyectos/${proyecto.id}/alcance`, { method: 'PUT', body: JSON.stringify(cuerpo) })
        : api<Proyecto>('/proyectos', { method: 'POST', body: JSON.stringify({ ...cuerpo, clienteId: clienteId || null }) })
    },
    onSuccess: (guardado) => {
      queryClient.setQueryData(['proyectos', guardado.id], guardado)
      queryClient.invalidateQueries({ queryKey: ['proyectos'] })
      toast(proyecto ? 'Alcance actualizado' : 'Proyecto creado en borrador')
      navegar(`/proyectos/${guardado.id}`)
    },
  })

  const errores = erroresDeFormulario(guardar.error)
  const formulario = useEnfocarError(guardar.error)
  const cambiar = (clave: string, campo: keyof HitoEditable, valor: string) =>
    setHitos((lista) => lista.map((h) => (h.clave === clave ? { ...h, [campo]: valor } : h)))
  // Vista previa: el total oficial lo calcula el servidor al guardar.
  const total = hitos.reduce((suma, h) => suma + (leerMonto(h.monto) || 0), 0)

  function enviar(e: FormEvent) {
    e.preventDefault()
    guardar.mutate()
  }

  return (
    <form ref={formulario} className="hoja" onSubmit={enviar} noValidate>
      <h1 className="titulo-hoja">{proyecto ? `Editar alcance · versión ${proyecto.ultimaVersion}` : 'Nuevo proyecto'}</h1>
      {(errores.general || errores.campo('hitos')) && (
        <p className="mensaje-error" role="alert">
          {errores.general ?? errores.campo('hitos')}
        </p>
      )}
      {!proyecto && (
        <div className={errores.campo('clienteId') ? 'campo error' : 'campo'}>
          <label htmlFor="clienteId">Cliente</label>
          <select id="clienteId" value={clienteId} onChange={(e) => setClienteId(e.target.value)} aria-invalid={errores.campo('clienteId') ? true : undefined} aria-describedby={errores.campo('clienteId') ? 'clienteId-error' : undefined}>
            <option value="">Elige un cliente</option>
            {clientes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.nombre}
              </option>
            ))}
          </select>
          {errores.campo('clienteId') && (
            <span id="clienteId-error" className="mensaje-error">
              {errores.campo('clienteId')}
            </span>
          )}
        </div>
      )}
      <Campo id="nombre" etiqueta="Nombre del proyecto" required maxLength={120} value={nombre} onChange={(e) => setNombre(e.target.value)} error={errores.campo('nombre')} ayuda="Ej.: Tienda en línea" />

      {hitos.map((h, i) => (
        <section key={h.clave} className="hito-form" aria-labelledby={`hito-${i}`}>
          <div className="cabeza">
            <h3 id={`hito-${i}`} className="rotulo">
              Hito {i + 1}
            </h3>
            {hitos.length > 1 && (
              <button type="button" className="btn texto" onClick={() => setHitos((lista) => lista.filter((x) => x.clave !== h.clave))}>
                Quitar
              </button>
            )}
          </div>
          <Campo id={`hito-${i}-titulo`} etiqueta="Título" maxLength={120} value={h.titulo} onChange={(e) => cambiar(h.clave, 'titulo', e.target.value)} error={errores.campo(`hitos[${i}].titulo`)} />
          <div className="fila">
            <Campo id={`hito-${i}-monto`} etiqueta="Monto (MXN)" className="num" inputMode="decimal" placeholder="0.00" value={h.monto} onChange={(e) => cambiar(h.clave, 'monto', e.target.value)} error={errores.campo(`hitos[${i}].monto`)} />
            <Campo id={`hito-${i}-fecha`} etiqueta="Fecha de entrega" type="date" value={h.fechaEntrega} onChange={(e) => cambiar(h.clave, 'fechaEntrega', e.target.value)} error={errores.campo(`hitos[${i}].fechaEntrega`)} />
          </div>
          <CampoArea id={`hito-${i}-entregable`} etiqueta="¿Qué se entrega?" maxLength={2000} value={h.entregable} onChange={(e) => cambiar(h.clave, 'entregable', e.target.value)} error={errores.campo(`hitos[${i}].entregable`)} />
        </section>
      ))}

      <button type="button" className="btn secundario" onClick={() => setHitos((lista) => [...lista, vacio()])} disabled={hitos.length >= 30}>
        Agregar hito
      </button>

      <div className="totales">
        <span className="final">Total</span>
        <span className="final monto">{dinero(total)}</span>
      </div>

      <div className="botonera">
        <button className="btn primario" type="submit" disabled={guardar.isPending}>
          {guardar.isPending ? 'Guardando…' : proyecto ? 'Guardar alcance' : 'Crear proyecto'}
        </button>
        <Link className="btn secundario" to={proyecto ? `/proyectos/${proyecto.id}` : '/proyectos'}>
          Cancelar
        </Link>
      </div>
    </form>
  )
}

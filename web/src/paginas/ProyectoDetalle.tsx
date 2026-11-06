import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link, useParams } from 'react-router'
import { api } from '../api/cliente'
import { mensajeDeError } from '../api/errores'
import { useProyecto, versionDe, type Proyecto, type Version } from '../api/proyectos'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { Sello } from '../componentes/Sello'
import { useToast } from '../componentes/toast'
import { dinero, fechaCorta, hoyIso } from '../formato'

type Pestana = 'hitos' | 'versiones'

export function ProyectoDetalle() {
  const { id } = useParams()
  const proyecto = useProyecto(id)
  const [pestana, setPestana] = useState<Pestana>('hitos')

  if (proyecto.isPending) return <Esqueleto renglones={6} />
  if (proyecto.isError) return <ErrorCarga error={proyecto.error} onReintentar={() => proyecto.refetch()} />

  const p = proyecto.data
  const actual = versionDe(p, p.versionActual)
  const ultima = versionDe(p, p.ultimaVersion)

  return (
    <>
      <div className="cabecera-proyecto">
        <div className="meta">{p.cliente.nombre}</div>
        <h1>{p.nombre}</h1>
        <div className="meta">{lineaDeEstado(p, actual)}</div>
      </div>

      {ultima.numero !== actual.numero && (
        <p className="aviso-linea">
          La versión {ultima.numero} está {ultima.estado === 'Borrador' ? 'en borrador' : 'esperando que el cliente la acepte'}. Mientras tanto rige la versión {actual.numero}.
        </p>
      )}

      <div className="pestanas" role="tablist">
        <button role="tab" aria-selected={pestana === 'hitos'} onClick={() => setPestana('hitos')}>
          Hitos
        </button>
        <button role="tab" aria-selected={pestana === 'versiones'} onClick={() => setPestana('versiones')}>
          Versiones
        </button>
      </div>

      {pestana === 'hitos' ? <HojaDeHitos proyecto={p} version={actual} /> : <Versiones proyecto={p} />}

      <Acciones proyecto={p} />
    </>
  )
}

function lineaDeEstado(p: Proyecto, v: Version) {
  if (p.estado === 'Cancelado') return `Cancelado · versión ${v.numero}`
  if (p.estado === 'Completado') return `Completado · versión ${v.numero}`
  if (v.estado === 'Aceptada') return `Versión ${v.numero} · aceptada el ${fechaCorta(v.aceptadaEn!)}`
  if (v.estado === 'Enviada') return `Versión ${v.numero} · enviada el ${fechaCorta(v.enviadaEn!)}, esperando respuesta`
  return `Versión ${v.numero} · borrador, aún no se envía`
}

export function HojaDeHitos({ proyecto, version }: { proyecto: Proyecto; version: Version }) {
  // Vencido: la fecha ya pasó y no se ha pagado. Solo cuenta con el alcance aceptado.
  const hoy = hoyIso()
  const cuentaVencido = proyecto.estado === 'Aceptado'
  return (
    <div className="hoja">
      {version.hitos.map((h) => (
        <div key={h.id} className="renglon">
          <b>{h.titulo}</b>
          <span className="monto">{dinero(h.monto)}</span>
          <small>
            Entrega: {fechaCorta(h.fechaEntrega)} · {h.entregable}
          </small>
          <Sello estado={h.estado} vencido={cuentaVencido && h.estado !== 'Pagado' && h.fechaEntrega < hoy} />
        </div>
      ))}
      <div className="totales">
        <span className="final">Total</span>
        <span className="final monto">{dinero(version.total)}</span>
      </div>
    </div>
  )
}

function Versiones({ proyecto }: { proyecto: Proyecto }) {
  const descripcion = (v: Version) =>
    v.estado === 'Aceptada'
      ? `Aceptada el ${fechaCorta(v.aceptadaEn!)}`
      : v.estado === 'Enviada'
        ? `Enviada el ${fechaCorta(v.enviadaEn!)}`
        : v.estado === 'Reemplazada'
          ? 'Reemplazada antes de aceptarse'
          : 'Borrador'
  return (
    <div className="hoja">
      {proyecto.versiones.map((v) => (
        <div key={v.numero} className="renglon">
          <b>Versión {v.numero}</b>
          <span className="monto">{dinero(v.total)}</span>
          <small>
            {descripcion(v)} · {v.hitos.length} {v.hitos.length === 1 ? 'hito' : 'hitos'}: {v.hitos.map((h) => `${h.titulo} (${dinero(h.monto)})`).join(', ')}
          </small>
        </div>
      ))}
      <p className="meta">Una versión aceptada no cambia: sus hitos y montos quedan congelados.</p>
    </div>
  )
}

function Acciones({ proyecto }: { proyecto: Proyecto }) {
  const queryClient = useQueryClient()
  const toast = useToast()
  const ultima = versionDe(proyecto, proyecto.ultimaVersion)
  const abierto = proyecto.estado !== 'Cancelado' && proyecto.estado !== 'Completado'

  const cancelar = useMutation({
    mutationFn: () => api<Proyecto>(`/proyectos/${proyecto.id}/cancelar`, { method: 'POST' }),
    onSuccess: (p) => {
      queryClient.setQueryData(['proyectos', p.id], p)
      queryClient.invalidateQueries({ queryKey: ['proyectos'] })
      toast('Proyecto cancelado. Su historial se conserva.')
    },
  })

  if (!abierto) return null
  return (
    <div className="botonera">
      {ultima.estado === 'Borrador' && (
        <Link className="btn primario" to={`/proyectos/${proyecto.id}/alcance`}>
          Editar alcance
        </Link>
      )}
      <button
        className="btn texto"
        disabled={cancelar.isPending}
        onClick={() => {
          if (window.confirm('¿Cancelar este proyecto? Se conserva su historial, pero ya no podrás cambiarlo.')) cancelar.mutate()
        }}
      >
        Cancelar proyecto
      </button>
      {cancelar.isError && (
        <p className="mensaje-error" role="alert">
          {mensajeDeError(cancelar.error)}
        </p>
      )}
    </div>
  )
}

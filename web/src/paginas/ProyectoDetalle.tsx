import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { api } from '../api/cliente'
import { erroresDeFormulario, mensajeDeError } from '../api/errores'
import { useProyecto, versionDe, type Hito, type Proyecto, type Version } from '../api/proyectos'
import { Campo } from '../componentes/Campo'
import { Dialogo } from '../componentes/Dialogo'
import { Compartir } from '../componentes/Compartir'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { Sello } from '../componentes/Sello'
import { useToast } from '../componentes/toast'
import { dinero, fechaCorta, hoyIso, leerMonto } from '../formato'

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

      <Compartir proyecto={p} />
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
  const vigente = proyecto.estado === 'Aceptado' && version.estado === 'Aceptada'
  return (
    <div className="hoja">
      {version.hitos.map((h) => (
        <RenglonHito key={h.id} proyecto={proyecto} hito={h} accionable={vigente} vencido={vigente && h.estado !== 'Pagado' && h.fechaEntrega < hoy} />
      ))}
      <div className="totales">
        {vigente && (
          <>
            <span>Pagado</span>
            <span className="monto">{dinero(proyecto.pagado)}</span>
            <span>Por cobrar</span>
            <span className="monto">{dinero(version.total - proyecto.pagado)}</span>
          </>
        )}
        <span className="final">Total</span>
        <span className="final monto">{dinero(version.total)}</span>
      </div>
    </div>
  )
}

function detalleDeHito(h: Hito) {
  if (h.estado === 'Pagado') return 'Pagado completo'
  if (h.estado === 'Aprobado' && h.pagado > 0) return `Pagado ${dinero(h.pagado)} de ${dinero(h.monto)}`
  if (h.estado === 'Aprobado' && h.aprobadoEn) return `Aprobado el ${fechaCorta(h.aprobadoEn)}`
  if (h.estado === 'Entregado' && h.entregadoEn) return `Entregado el ${fechaCorta(h.entregadoEn)}, esperando revisión`
  if (h.estado === 'CambiosSolicitados') return 'El cliente pidió cambios'
  return `Entrega: ${fechaCorta(h.fechaEntrega)} · ${h.entregable}`
}

function RenglonHito({ proyecto, hito, accionable, vencido }: { proyecto: Proyecto; hito: Hito; accionable: boolean; vencido: boolean }) {
  const [entregando, setEntregando] = useState(false)
  const [pagando, setPagando] = useState(false)
  const puedeEntregar = accionable && (hito.estado === 'Pendiente' || hito.estado === 'CambiosSolicitados')
  const puedePagar = accionable && hito.estado === 'Aprobado'
  return (
    <div className="renglon">
      <b>{hito.titulo}</b>
      <span className="monto">{dinero(hito.monto)}</span>
      <small>
        {detalleDeHito(hito)}
        {hito.evidenciaUrl && hito.estado !== 'Pendiente' && (
          <>
            {' · '}
            <a href={hito.evidenciaUrl} target="_blank" rel="noopener noreferrer">
              ver entrega ↗
            </a>
          </>
        )}
      </small>
      <Sello estado={hito.estado} vencido={vencido} />
      {hito.estado === 'CambiosSolicitados' && hito.comentarioCambios && <p className="comentario">«{hito.comentarioCambios}»</p>}
      {puedeEntregar && (
        <div className="acciones">
          <button className="btn secundario" onClick={() => setEntregando(true)}>
            {hito.estado === 'CambiosSolicitados' ? 'Entregar de nuevo' : 'Marcar como entregado'}
          </button>
        </div>
      )}
      {puedePagar && (
        <div className="acciones">
          <button className="btn primario" onClick={() => setPagando(true)}>
            Registrar pago
          </button>
        </div>
      )}
      <Dialogo titulo="Registrar pago" abierto={pagando} onCerrar={() => setPagando(false)}>
        <FormularioPago proyectoId={proyecto.id} hito={hito} onListo={() => setPagando(false)} />
      </Dialogo>
      <Dialogo titulo={`Entregar «${hito.titulo}»`} abierto={entregando} onCerrar={() => setEntregando(false)}>
        <FormularioEntrega proyectoId={proyecto.id} hito={hito} onListo={() => setEntregando(false)} />
      </Dialogo>
    </div>
  )
}

function FormularioEntrega({ proyectoId, hito, onListo }: { proyectoId: string; hito: Hito; onListo: () => void }) {
  const queryClient = useQueryClient()
  const toast = useToast()
  const [url, setUrl] = useState(hito.evidenciaUrl ?? 'https://')
  const entregar = useMutation({
    mutationFn: () => api<Proyecto>(`/proyectos/${proyectoId}/hitos/${hito.id}/entregar`, { method: 'POST', body: JSON.stringify({ evidenciaUrl: url }) }),
    onSuccess: (p) => {
      queryClient.setQueryData(['proyectos', p.id], p)
      queryClient.invalidateQueries({ queryKey: ['proyectos'] })
      toast(`«${hito.titulo}» entregado. Tu cliente ya puede revisarlo.`)
      onListo()
    },
  })
  const errores = erroresDeFormulario(entregar.error)

  function enviar(e: FormEvent) {
    e.preventDefault()
    entregar.mutate()
  }

  return (
    <form onSubmit={enviar} noValidate>
      {errores.general && (
        <p className="mensaje-error" role="alert">
          {errores.general}
        </p>
      )}
      <Campo id="evidenciaUrl" etiqueta="Enlace a la entrega" type="url" inputMode="url" autoFocus value={url} onChange={(e) => setUrl(e.target.value)} error={errores.campo('evidenciaUrl')} ayuda="El sitio publicado, una carpeta compartida o un archivo. Debe empezar con https://" />
      <div className="botonera columna">
        <button className="btn primario ancho" type="submit" disabled={entregar.isPending}>
          {entregar.isPending ? 'Entregando…' : 'Entregar'}
        </button>
        <button className="btn secundario ancho" type="button" onClick={onListo}>
          Cancelar
        </button>
      </div>
    </form>
  )
}

function FormularioPago({ proyectoId, hito, onListo }: { proyectoId: string; hito: Hito; onListo: () => void }) {
  const queryClient = useQueryClient()
  const toast = useToast()
  const pendiente = Math.round((hito.monto - hito.pagado) * 100) / 100
  const [datos, setDatos] = useState({ monto: pendiente.toFixed(2), fecha: hoyIso(), nota: '' })
  const [errorMonto, setErrorMonto] = useState<string>()
  const monto = leerMonto(datos.monto)
  const pagar = useMutation({
    mutationFn: () => api<Proyecto>(`/proyectos/${proyectoId}/hitos/${hito.id}/pagos`, { method: 'POST', body: JSON.stringify({ monto, fecha: datos.fecha || null, nota: datos.nota }) }),
    onSuccess: (p) => {
      queryClient.setQueryData(['proyectos', p.id], p)
      queryClient.invalidateQueries({ queryKey: ['proyectos'] })
      queryClient.invalidateQueries({ queryKey: ['cobros'] })
      toast(`Pago registrado · ${dinero(monto)}`)
      onListo()
    },
  })
  const errores = erroresDeFormulario(pagar.error)

  function enviar(e: FormEvent) {
    e.preventDefault()
    // Se revisa antes de enviar para dar el mensaje al instante; el servidor vuelve a validarlo.
    if (!(monto > 0)) return setErrorMonto('Escribe el monto recibido.')
    if (monto > pendiente) return setErrorMonto(`No puede ser mayor que lo pendiente: ${dinero(pendiente)}`)
    pagar.mutate()
  }

  return (
    <form onSubmit={enviar} noValidate>
      <div className="meta">
        {hito.titulo} · {hito.aprobadoEn ? `aprobado el ${fechaCorta(hito.aprobadoEn)}` : 'aprobado'}
      </div>
      {errores.general && (
        <p className="mensaje-error" role="alert">
          {errores.general}
        </p>
      )}
      <Campo
        id="monto"
        etiqueta="Monto recibido (MXN)"
        className="num"
        inputMode="decimal"
        autoFocus
        value={datos.monto}
        onChange={(e) => {
          setDatos((d) => ({ ...d, monto: e.target.value }))
          setErrorMonto(undefined)
        }}
        error={errorMonto ?? errores.campo('monto')}
        ayuda={`Pendiente: ${dinero(pendiente)}`}
      />
      <Campo id="fecha" etiqueta="Fecha del pago" type="date" value={datos.fecha} onChange={(e) => setDatos((d) => ({ ...d, fecha: e.target.value }))} error={errores.campo('fecha')} />
      <Campo id="nota" etiqueta="Nota (opcional)" maxLength={200} placeholder="Transferencia, referencia…" value={datos.nota} onChange={(e) => setDatos((d) => ({ ...d, nota: e.target.value }))} error={errores.campo('nota')} />
      <div className="botonera columna">
        <button className="btn primario ancho" type="submit" disabled={pagar.isPending}>
          {pagar.isPending ? 'Registrando…' : `Registrar ${monto > 0 ? dinero(monto) : 'pago'}`}
        </button>
        <button className="btn secundario ancho" type="button" onClick={onListo}>
          Cancelar
        </button>
      </div>
    </form>
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

  const navegar = useNavigate()
  const nuevaVersion = useMutation({
    mutationFn: () => api<Proyecto>(`/proyectos/${proyecto.id}/versiones`, { method: 'POST' }),
    onSuccess: (p) => {
      queryClient.setQueryData(['proyectos', p.id], p)
      queryClient.invalidateQueries({ queryKey: ['proyectos'] })
      toast(`Versión ${p.ultimaVersion} creada en borrador`)
      navegar(`/proyectos/${p.id}/alcance`)
    },
  })

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
        <Link className="btn secundario" to={`/proyectos/${proyecto.id}/alcance`}>
          Editar alcance
        </Link>
      )}
      {ultima.estado !== 'Borrador' && (
        <button
          className="btn secundario"
          disabled={nuevaVersion.isPending}
          onClick={() => {
            const aviso = ultima.estado === 'Enviada' ? ' La versión enviada dejará de poder aceptarse.' : ''
            if (window.confirm(`¿Crear la versión ${ultima.numero + 1}? Tu cliente tendrá que aceptarla.${aviso}`)) nuevaVersion.mutate()
          }}
        >
          Cambiar alcance
        </button>
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
      {(cancelar.isError || nuevaVersion.isError) && (
        <p className="mensaje-error" role="alert">
          {mensajeDeError(cancelar.error ?? nuevaVersion.error)}
        </p>
      )}
    </div>
  )
}

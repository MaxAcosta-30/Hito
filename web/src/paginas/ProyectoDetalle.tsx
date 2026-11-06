import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useRef, useState, type FormEvent, type KeyboardEvent } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router'
import { api } from '../api/cliente'
import { erroresDeFormulario, mensajeDeError } from '../api/errores'
import { useEventos, useProyecto, versionDe, type Hito, type Proyecto, type Version } from '../api/proyectos'
import { Campo } from '../componentes/Campo'
import { Dialogo } from '../componentes/Dialogo'
import { Compartir } from '../componentes/Compartir'
import { Confirmar } from '../componentes/Confirmar'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { Sello } from '../componentes/Sello'
import { useToast } from '../componentes/toast'
import { useEnfocarError } from '../componentes/useEnfocarError'
import { dinero, fechaCorta, fechaLarga, horaCorta, hoyIso, leerMonto } from '../formato'

const pestanas = [
  { clave: 'hitos', texto: 'Hitos' },
  { clave: 'historial', texto: 'Historial' },
  { clave: 'versiones', texto: 'Versiones' },
] as const
type Pestana = (typeof pestanas)[number]['clave']

export function ProyectoDetalle() {
  const { id } = useParams()
  const proyecto = useProyecto(id)
  // La pestaña vive en la URL: se puede recargar o compartir. Con replace, cambiar de pestaña no llena el historial.
  const [parametros, setParametros] = useSearchParams()
  const pestana: Pestana = pestanas.find((t) => t.clave === parametros.get('pestana'))?.clave ?? 'hitos'
  const botones = useRef<(HTMLButtonElement | null)[]>([])
  const elegir = (clave: Pestana) => setParametros(clave === 'hitos' ? {} : { pestana: clave }, { replace: true })

  // Patrón ARIA de pestañas: flechas, Inicio y Fin mueven el foco y la selección.
  function alTeclear(e: KeyboardEvent, i: number) {
    const destino = { ArrowRight: i + 1, ArrowLeft: i - 1, Home: 0, End: pestanas.length - 1 }[e.key]
    if (destino === undefined) return
    e.preventDefault()
    const j = (destino + pestanas.length) % pestanas.length
    elegir(pestanas[j].clave)
    botones.current[j]?.focus()
  }

  if (proyecto.isPending) return <Esqueleto renglones={6} />
  if (proyecto.isError) return <ErrorCarga error={proyecto.error} onReintentar={() => proyecto.refetch()} />

  const p = proyecto.data
  const actual = versionDe(p, p.versionActual)
  const ultima = versionDe(p, p.ultimaVersion)

  // Exportar a PDF con el diálogo de impresión del navegador ("Guardar como PDF").
  // El título de la página da nombre al archivo; se restaura al cerrar el diálogo.
  function exportar() {
    const titulo = document.title
    document.title = `${p.nombre} · ${p.cliente.nombre}`
    window.addEventListener('afterprint', () => (document.title = titulo), { once: true })
    window.print()
  }

  return (
    <>
      <div className="cabecera-proyecto">
        <div className="meta">{p.cliente.nombre}</div>
        <h1>{p.nombre}</h1>
        <div className="meta">{lineaDeEstado(p, actual)}</div>
        <button className="btn secundario no-imprimir" onClick={exportar}>
          Exportar PDF
        </button>
      </div>

      {ultima.numero !== actual.numero && (
        <p className="aviso-linea">
          La versión {ultima.numero} está {ultima.estado === 'Borrador' ? 'en borrador' : 'esperando que el cliente la acepte'}. Mientras tanto rige la versión {actual.numero}.
        </p>
      )}

      <div className="pestanas" role="tablist" aria-label="Detalle del proyecto">
        {pestanas.map((t, i) => (
          <button
            key={t.clave}
            ref={(b) => {
              botones.current[i] = b
            }}
            id={`pestana-${t.clave}`}
            role="tab"
            aria-selected={pestana === t.clave}
            aria-controls="panel-proyecto"
            tabIndex={pestana === t.clave ? 0 : -1}
            onClick={() => elegir(t.clave)}
            onKeyDown={(e) => alTeclear(e, i)}
          >
            {t.texto}
          </button>
        ))}
      </div>

      {/* El PDF siempre resume los hitos, aunque se exporte desde otra pestaña. */}
      <div id="panel-proyecto" role="tabpanel" aria-labelledby={`pestana-${pestana}`} className={pestana === 'hitos' ? undefined : 'no-imprimir'}>
        {pestana === 'hitos' ? <HojaDeHitos proyecto={p} version={actual} /> : pestana === 'historial' ? <Historial proyectoId={p.id} /> : <Versiones proyecto={p} />}
      </div>

      {pestana !== 'hitos' && (
        <div className="solo-impresion">
          <HojaDeHitos proyecto={p} version={actual} />
        </div>
      )}
      <p className="solo-impresion meta">Resumen generado el {fechaLarga(hoyIso())} con Hito.</p>

      <div className="no-imprimir">
        <Compartir proyecto={p} />
        <Acciones proyecto={p} />
      </div>
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
      queryClient.invalidateQueries({ queryKey: ['cobros'] })
      toast(`«${hito.titulo}» entregado. Tu cliente ya puede revisarlo.`)
      onListo()
    },
  })
  const errores = erroresDeFormulario(entregar.error)
  const formEntrega = useEnfocarError(entregar.error)

  function enviar(e: FormEvent) {
    e.preventDefault()
    entregar.mutate()
  }

  return (
    <form ref={formEntrega} onSubmit={enviar} noValidate>
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
  const formPago = useEnfocarError(pagar.error)

  function enviar(e: FormEvent) {
    e.preventDefault()
    // Se revisa antes de enviar para dar el mensaje al instante; el servidor vuelve a validarlo.
    const error = !(monto > 0) ? 'Escribe el monto recibido.' : monto > pendiente ? `No puede ser mayor que lo pendiente: ${dinero(pendiente)}` : undefined
    if (error) {
      setErrorMonto(error)
      return document.getElementById('monto')?.focus()
    }
    pagar.mutate()
  }

  return (
    <form ref={formPago} onSubmit={enviar} noValidate>
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

// Solo lectura: el historial no se edita ni se borra.
function Historial({ proyectoId }: { proyectoId: string }) {
  const eventos = useEventos(proyectoId, true)
  if (eventos.isPending) return <Esqueleto renglones={5} />
  if (eventos.isError) return <ErrorCarga error={eventos.error} onReintentar={() => eventos.refetch()} />
  return (
    <>
      <ol className="hoja historial">
        {eventos.data.map((e, i) => (
          <li key={i} className="evento">
            <time dateTime={e.fecha}>
              {fechaCorta(e.fecha)}
              <br />
              {horaCorta(e.fecha)}
            </time>
            <div>
              <span className="quien">{e.quien}</span> {e.descripcion}
            </div>
          </li>
        ))}
      </ol>
      <p className="meta">Los eventos no se pueden editar ni borrar.</p>
    </>
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
  const [confirmando, setConfirmando] = useState<'version' | 'cancelar' | null>(null)

  const navegar = useNavigate()
  const nuevaVersion = useMutation({
    mutationFn: () => api<Proyecto>(`/proyectos/${proyecto.id}/versiones`, { method: 'POST' }),
    onSuccess: (p) => {
      queryClient.setQueryData(['proyectos', p.id], p)
      queryClient.invalidateQueries({ queryKey: ['proyectos'] })
      queryClient.invalidateQueries({ queryKey: ['cobros'] })
      toast(`Versión ${p.ultimaVersion} creada en borrador`)
      navegar(`/proyectos/${p.id}/alcance`)
    },
    onError: () => setConfirmando(null),
  })

  const cancelar = useMutation({
    mutationFn: () => api<Proyecto>(`/proyectos/${proyecto.id}/cancelar`, { method: 'POST' }),
    onSuccess: (p) => {
      queryClient.setQueryData(['proyectos', p.id], p)
      queryClient.invalidateQueries({ queryKey: ['proyectos'] })
      queryClient.invalidateQueries({ queryKey: ['cobros'] })
      setConfirmando(null)
      toast('Proyecto cancelado. Su historial se conserva.')
    },
    onError: () => setConfirmando(null),
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
        <button className="btn secundario" disabled={nuevaVersion.isPending} onClick={() => setConfirmando('version')}>
          Cambiar alcance
        </button>
      )}
      {/* Destructiva: separada del resto y en rojo. */}
      <button className="btn peligro-contorno separado" disabled={cancelar.isPending} onClick={() => setConfirmando('cancelar')}>
        Cancelar proyecto
      </button>
      {(cancelar.isError || nuevaVersion.isError) && (
        <p className="mensaje-error" role="alert">
          {mensajeDeError(cancelar.error ?? nuevaVersion.error)}
        </p>
      )}
      <Confirmar
        titulo={`¿Crear la versión ${ultima.numero + 1}?`}
        textoConfirmar={`Crear versión ${ultima.numero + 1}`}
        abierto={confirmando === 'version'}
        pendiente={nuevaVersion.isPending}
        onConfirmar={() => nuevaVersion.mutate()}
        onCerrar={() => setConfirmando(null)}
      >
        Tu cliente tendrá que aceptarla. Mientras tanto sigue vigente la versión {proyecto.versionActual}.
        {ultima.estado === 'Enviada' && ' La versión que ya enviaste dejará de poder aceptarse.'}
      </Confirmar>
      <Confirmar
        titulo="¿Cancelar este proyecto?"
        textoConfirmar="Sí, cancelar proyecto"
        peligro
        abierto={confirmando === 'cancelar'}
        pendiente={cancelar.isPending}
        onConfirmar={() => cancelar.mutate()}
        onCerrar={() => setConfirmando(null)}
      >
        No se puede deshacer. Ya no podrás cambiar el alcance, entregar ni registrar pagos, y el enlace del cliente dejará de funcionar. El historial y los pagos registrados se conservan.
      </Confirmar>
    </div>
  )
}

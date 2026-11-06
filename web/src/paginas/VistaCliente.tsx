import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useParams } from 'react-router'
import { api, ErrorApi } from '../api/cliente'
import { mensajeDeError } from '../api/errores'
import { useVistaCliente, type HitoPublico, type VersionPublica, type VistaCliente as Vista } from '../api/publico'
import { CampoArea } from '../componentes/Campo'
import { Dialogo } from '../componentes/Dialogo'
import { ErrorCarga } from '../componentes/Estados'
import { Sello } from '../componentes/Sello'
import { dinero, fechaCorta } from '../formato'

// Lo que ve el cliente desde su enlace, normalmente en el celular y sin cuenta.
export function VistaCliente() {
  const { token = '' } = useParams()
  const vista = useVistaCliente(token)
  const queryClient = useQueryClient()
  const [exito, setExito] = useState<{ sello: string; texto: string } | null>(null)
  const alResponder = (nueva: Vista, sello: string, texto: string) => {
    queryClient.setQueryData(['publico', token], nueva)
    setExito({ sello, texto })
    window.scrollTo({ top: 0 })
  }
  const aceptar = useMutation({
    mutationFn: (version: number) => api<Vista>(`/publico/${token}/aceptar`, { method: 'POST', body: JSON.stringify({ version }) }),
    onSuccess: (nueva) =>
      alResponder(nueva, 'ACEPTADO', `Aceptaste la versión ${nueva.vigente!.numero} por ${dinero(nueva.vigente!.total)}. ${nueva.freelancer} ya lo ve en su panel. Si algo cambia, recibirás una versión nueva para aceptar.`),
    // Si el freelancer cambió el alcance mientras tanto (409), se recarga para mostrar lo vigente.
    onError: () => queryClient.invalidateQueries({ queryKey: ['publico', token] }),
  })

  if (vista.isPending) {
    return (
      <main className="contenido cliente" aria-busy="true">
        <div className="esqueleto corto" />
        <div className="esqueleto" style={{ height: 28 }} />
        <div className="hoja">
          <div className="esqueleto" />
          <div className="esqueleto corto" />
          <div className="esqueleto" />
        </div>
      </main>
    )
  }

  if (vista.isError) {
    if (vista.error instanceof ErrorApi && vista.error.estado === 404) return <EnlaceNoValido />
    return (
      <main className="contenido cliente">
        <ErrorCarga error={vista.error} onReintentar={() => vista.refetch()} />
      </main>
    )
  }

  const v = vista.data
  const cancelado = v.estado === 'Cancelado'
  return (
    <main className="contenido cliente">
      <header className="cliente-cabecera">
        <div className="meta">{v.porAceptar ? `${v.freelancer} te envió este alcance` : `Proyecto con ${v.freelancer}`}</div>
        <h1>{v.proyecto}</h1>
        <div className="meta">Para {v.cliente}</div>
      </header>

      {exito && (
        <div className="hoja exito" role="status">
          <span className={exito.sello === 'CAMBIOS' ? 'sello cambios grande' : 'sello aprobado grande'}>{exito.sello}</span>
          <h3>Gracias, quedó registrado</h3>
          <p className="meta">{exito.texto}</p>
        </div>
      )}

      {cancelado && <p className="aviso-linea">{v.freelancer} canceló este proyecto. Lo que ya se registró se conserva.</p>}

      {v.porAceptar && (
        <section aria-labelledby="por-aceptar">
          <h2 id="por-aceptar" className="rotulo titulo-seccion">
            {v.vigente ? `Nueva versión ${v.porAceptar.numero} · espera tu respuesta` : `Versión ${v.porAceptar.numero} · espera tu respuesta`}
          </h2>
          <Hoja version={v.porAceptar} conSellos={false} />
        </section>
      )}

      {v.vigente && (
        <section aria-labelledby="vigente">
          <h2 id="vigente" className="rotulo titulo-seccion">
            Versión {v.vigente.numero} · aceptada el {fechaCorta(v.vigente.aceptadaEn!)}
          </h2>
          <Hoja version={v.vigente} conSellos token={token} freelancer={v.freelancer} onRespuesta={alResponder} />
        </section>
      )}

      {!v.porAceptar && !v.vigente && (
        <div className="hoja vacio">
          <h3>Aún no hay nada que revisar</h3>
          <p className="meta">{v.freelancer} está preparando una nueva versión del alcance. Vuelve a abrir este enlace más tarde.</p>
        </div>
      )}

      <p className="meta pie-cliente">Este enlace vence el {fechaCorta(v.enlaceExpiraEn)}.</p>

      {v.porAceptar && !cancelado && (
        <div className="barra-accion">
          <div>
            {aceptar.isError ? (
              <p className="mensaje-error" role="alert">
                {mensajeDeError(aceptar.error)}
              </p>
            ) : (
              <p className="nota">Al aceptar, los hitos y montos quedan fijos. Si algo cambia, recibirás una nueva versión.</p>
            )}
            <button className="btn primario ancho" onClick={() => aceptar.mutate(v.porAceptar!.numero)} disabled={aceptar.isPending}>
              {aceptar.isPending ? 'Aceptando…' : `Aceptar alcance · ${dinero(v.porAceptar.total)}`}
            </button>
          </div>
        </div>
      )}
    </main>
  )
}

type AlResponder = (nueva: Vista, sello: string, texto: string) => void

function Hoja({ version, conSellos, token, freelancer, onRespuesta }: { version: VersionPublica; conSellos: boolean; token?: string; freelancer?: string; onRespuesta?: AlResponder }) {
  return (
    <div className="hoja">
      {version.hitos.map((h) => (
        <div key={h.id} className="renglon">
          <b>{h.titulo}</b>
          <span className="monto">{dinero(h.monto)}</span>
          <small>{h.estado === 'Entregado' && h.entregadoEn ? `Entregado el ${fechaCorta(h.entregadoEn)}` : `Entrega: ${fechaCorta(h.fechaEntrega)} · ${h.entregable}`}</small>
          {conSellos && <Sello estado={h.estado} />}
          {h.estado === 'Entregado' && token && onRespuesta && <RevisarEntrega hito={h} token={token} freelancer={freelancer!} onRespuesta={onRespuesta} />}
        </div>
      ))}
      <div className="totales">
        <span className="final">Total</span>
        <span className="final monto">{dinero(version.total)}</span>
      </div>
    </div>
  )
}

// Revisar una entrega: abrir la evidencia y aprobar o pedir cambios con un comentario obligatorio.
function RevisarEntrega({ hito, token, freelancer, onRespuesta }: { hito: HitoPublico; token: string; freelancer: string; onRespuesta: AlResponder }) {
  const [pidiendo, setPidiendo] = useState(false)
  const aprobar = useMutation({
    mutationFn: () => api<Vista>(`/publico/${token}/hitos/${hito.id}/aprobar`, { method: 'POST' }),
    onSuccess: (nueva) => onRespuesta(nueva, 'APROBADO', `${freelancer} verá tu aprobación de «${hito.titulo}» (${dinero(hito.monto)}).`),
  })
  return (
    <div className="acciones revisar">
      {hito.evidenciaUrl && (
        <a className="btn secundario ancho" href={hito.evidenciaUrl} target="_blank" rel="noopener noreferrer">
          Ver la entrega ↗
        </a>
      )}
      {aprobar.isError && (
        <p className="mensaje-error" role="alert">
          {mensajeDeError(aprobar.error)}
        </p>
      )}
      <button className="btn primario ancho" onClick={() => aprobar.mutate()} disabled={aprobar.isPending}>
        {aprobar.isPending ? 'Aprobando…' : 'Aprobar entrega'}
      </button>
      <button className="btn secundario ancho" onClick={() => setPidiendo(true)}>
        Pedir cambios
      </button>
      <Dialogo titulo="¿Qué hay que cambiar?" abierto={pidiendo} onCerrar={() => setPidiendo(false)}>
        <PedirCambios
          hito={hito}
          token={token}
          freelancer={freelancer}
          onListo={(nueva) => {
            setPidiendo(false)
            onRespuesta(nueva, 'CAMBIOS', `${freelancer} recibió tus comentarios sobre «${hito.titulo}» y te avisará cuando lo entregue de nuevo.`)
          }}
          onCancelar={() => setPidiendo(false)}
        />
      </Dialogo>
    </div>
  )
}

function PedirCambios({ hito, token, freelancer, onListo, onCancelar }: { hito: HitoPublico; token: string; freelancer: string; onListo: (v: Vista) => void; onCancelar: () => void }) {
  const [comentario, setComentario] = useState('')
  const [faltaComentario, setFaltaComentario] = useState(false)
  const enviar = useMutation({
    mutationFn: () => api<Vista>(`/publico/${token}/hitos/${hito.id}/cambios`, { method: 'POST', body: JSON.stringify({ comentario }) }),
    onSuccess: onListo,
  })

  function alEnviar(e: FormEvent) {
    e.preventDefault()
    // El comentario es obligatorio: se valida antes de enviar (y el servidor lo vuelve a validar).
    if (!comentario.trim()) return setFaltaComentario(true)
    enviar.mutate()
  }

  return (
    <form onSubmit={alEnviar} noValidate>
      {enviar.isError && (
        <p className="mensaje-error" role="alert">
          {mensajeDeError(enviar.error)}
        </p>
      )}
      <CampoArea
        id="comentario"
        etiqueta={`Comentario para ${freelancer}`}
        rows={4}
        maxLength={1000}
        autoFocus
        value={comentario}
        onChange={(e) => {
          setComentario(e.target.value)
          setFaltaComentario(false)
        }}
        error={faltaComentario ? `Escribe qué hay que cambiar para que ${freelancer} pueda corregirlo.` : undefined}
      />
      <div className="botonera columna">
        <button className="btn primario ancho" type="submit" disabled={enviar.isPending}>
          {enviar.isPending ? 'Enviando…' : 'Enviar cambios'}
        </button>
        <button className="btn secundario ancho" type="button" onClick={onCancelar}>
          Volver
        </button>
      </div>
    </form>
  )
}

function EnlaceNoValido() {
  return (
    <main className="contenido cliente">
      <div className="hoja vacio">
        <h3>Este enlace ya no es válido</h3>
        <p className="meta">Puede haber expirado o el freelancer lo desactivó. Pídele uno nuevo.</p>
      </div>
    </main>
  )
}

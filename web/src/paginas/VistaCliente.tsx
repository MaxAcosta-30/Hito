import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useParams } from 'react-router'
import { api, ErrorApi } from '../api/cliente'
import { mensajeDeError } from '../api/errores'
import { useVistaCliente, type VersionPublica, type VistaCliente as Vista } from '../api/publico'
import { ErrorCarga } from '../componentes/Estados'
import { Sello } from '../componentes/Sello'
import { dinero, fechaCorta } from '../formato'

// Lo que ve el cliente desde su enlace, normalmente en el celular y sin cuenta.
export function VistaCliente() {
  const { token = '' } = useParams()
  const vista = useVistaCliente(token)
  const queryClient = useQueryClient()
  const aceptar = useMutation({
    mutationFn: (version: number) => api<Vista>(`/publico/${token}/aceptar`, { method: 'POST', body: JSON.stringify({ version }) }),
    onSuccess: (nueva) => queryClient.setQueryData(['publico', token], nueva),
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

      {aceptar.isSuccess && v.vigente && (
        <div className="hoja exito" role="status">
          <span className="sello aprobado grande">ACEPTADO</span>
          <h3>Gracias, quedó registrado</h3>
          <p className="meta">
            Aceptaste la versión {v.vigente.numero} por {dinero(v.vigente.total)}. {v.freelancer} ya lo ve en su panel. Si algo cambia, recibirás una versión nueva para aceptar.
          </p>
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
          <Hoja version={v.vigente} conSellos />
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

function Hoja({ version, conSellos }: { version: VersionPublica; conSellos: boolean }) {
  return (
    <div className="hoja">
      {version.hitos.map((h) => (
        <div key={h.id} className="renglon">
          <b>{h.titulo}</b>
          <span className="monto">{dinero(h.monto)}</span>
          <small>
            Entrega: {fechaCorta(h.fechaEntrega)} · {h.entregable}
          </small>
          {conSellos && <Sello estado={h.estado} />}
        </div>
      ))}
      <div className="totales">
        <span className="final">Total</span>
        <span className="final monto">{dinero(version.total)}</span>
      </div>
    </div>
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

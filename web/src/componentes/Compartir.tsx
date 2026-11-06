import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { api } from '../api/cliente'
import { mensajeDeError } from '../api/errores'
import { urlDelCliente, type EnlaceCreado, type Proyecto } from '../api/proyectos'
import { fechaCorta } from '../formato'
import { useToast } from './toast'

// Enviar, ver, renovar y revocar el enlace privado del cliente.
export function Compartir({ proyecto }: { proyecto: Proyecto }) {
  const queryClient = useQueryClient()
  const toast = useToast()
  const [creado, setCreado] = useState<EnlaceCreado | null>(null)
  const borrador = proyecto.versiones.find((v) => v.numero === proyecto.ultimaVersion)!.estado === 'Borrador'

  const alCrear = (resultado: EnlaceCreado) => {
    setCreado(resultado)
    queryClient.setQueryData(['proyectos', proyecto.id], resultado.proyecto)
    queryClient.invalidateQueries({ queryKey: ['proyectos'] })
  }
  const enviar = useMutation({
    mutationFn: () => api<EnlaceCreado>(`/proyectos/${proyecto.id}/enviar`, { method: 'POST' }),
    onSuccess: (r) => {
      alCrear(r)
      toast(`Versión ${r.proyecto.ultimaVersion} enviada. Comparte el enlace con tu cliente.`)
    },
  })
  const nuevo = useMutation({
    mutationFn: () => api<EnlaceCreado>(`/proyectos/${proyecto.id}/enlace`, { method: 'POST' }),
    onSuccess: (r) => {
      alCrear(r)
      toast('Enlace nuevo creado. El anterior ya no funciona.')
    },
  })
  const revocar = useMutation({
    mutationFn: () => api<void>(`/proyectos/${proyecto.id}/enlace/revocar`, { method: 'POST' }),
    onSuccess: () => {
      setCreado(null)
      queryClient.invalidateQueries({ queryKey: ['proyectos', proyecto.id] })
      toast('Enlace revocado. Tu cliente ya no puede abrirlo.')
    },
  })
  const error = enviar.error ?? nuevo.error ?? revocar.error

  if (proyecto.estado === 'Cancelado' || proyecto.estado === 'Completado') return null

  return (
    <div className="hoja">
      <div className="rotulo">Enlace del cliente</div>
      {creado ? (
        <EnlaceRecienCreado creado={creado} />
      ) : proyecto.enlace ? (
        <p className="meta">Activo hasta el {fechaCorta(proyecto.enlace.expiraEn)}. Por seguridad no se puede volver a mostrar; si tu cliente lo perdió, genera uno nuevo.</p>
      ) : (
        <p className="meta">{borrador ? 'Cuando el alcance esté listo, envíalo para que tu cliente lo acepte.' : 'No hay un enlace activo.'}</p>
      )}
      {error && (
        <p className="mensaje-error" role="alert">
          {mensajeDeError(error)}
        </p>
      )}
      <div className="botonera">
        {borrador ? (
          <button className="btn primario" onClick={() => enviar.mutate()} disabled={enviar.isPending}>
            {enviar.isPending ? 'Enviando…' : `Enviar versión ${proyecto.ultimaVersion} al cliente`}
          </button>
        ) : (
          proyecto.estado !== 'Borrador' && (
            <button className="btn secundario" onClick={() => nuevo.mutate()} disabled={nuevo.isPending}>
              Generar enlace nuevo
            </button>
          )
        )}
        {(proyecto.enlace || creado) && (
          <button className="btn texto" onClick={() => revocar.mutate()} disabled={revocar.isPending}>
            Revocar enlace
          </button>
        )}
      </div>
    </div>
  )
}

function EnlaceRecienCreado({ creado }: { creado: EnlaceCreado }) {
  const toast = useToast()
  const url = urlDelCliente(creado.token)
  const mensaje = `Hola, te comparto el alcance de "${creado.proyecto.nombre}" para que lo revises: ${url}`

  async function copiar() {
    try {
      await navigator.clipboard.writeText(url)
      toast('Enlace copiado')
    } catch {
      toast('No se pudo copiar. Selecciona el enlace y cópialo a mano.')
    }
  }

  return (
    <>
      <div className="campo">
        <label htmlFor="enlace">Comparte este enlace (vence el {fechaCorta(creado.expiraEn)})</label>
        <input id="enlace" readOnly value={url} onFocus={(e) => e.target.select()} />
        <span className="ayuda">Solo se muestra ahora. Quien lo tenga puede ver el proyecto, así que compártelo solo con tu cliente.</span>
      </div>
      <div className="botonera">
        <button className="btn primario" onClick={copiar}>
          Copiar enlace
        </button>
        <a className="btn secundario" href={`https://wa.me/?text=${encodeURIComponent(mensaje)}`} target="_blank" rel="noopener noreferrer">
          Enviar por WhatsApp
        </a>
      </div>
    </>
  )
}

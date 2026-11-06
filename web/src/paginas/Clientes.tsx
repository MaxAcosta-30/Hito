import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { api } from '../api/cliente'
import { useClientes, type Cliente } from '../api/clientes'
import { mensajeDeError } from '../api/errores'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { useToast } from '../componentes/toast'

export function Clientes() {
  const [archivados, setArchivados] = useState(false)
  const clientes = useClientes(archivados)

  return (
    <>
      <div className="encabezado">
        <h1>Clientes</h1>
        <Link className="btn primario" to="/clientes/nuevo">
          Nuevo cliente
        </Link>
      </div>
      <div className="pestanas" role="group" aria-label="Mostrar clientes">
        <button aria-pressed={!archivados} onClick={() => setArchivados(false)}>
          Activos
        </button>
        <button aria-pressed={archivados} onClick={() => setArchivados(true)}>
          Archivados
        </button>
      </div>
      {clientes.isPending ? (
        <Esqueleto />
      ) : clientes.isError ? (
        <ErrorCarga error={clientes.error} onReintentar={() => clientes.refetch()} />
      ) : clientes.data.length === 0 ? (
        <div className="hoja vacio">
          {archivados ? (
            <>
              <h3>No hay clientes archivados</h3>
              <p className="meta">Cuando archives un cliente aparecerá aquí, con todo su historial.</p>
            </>
          ) : (
            <>
              <h3>Aún no tienes clientes</h3>
              <p className="meta">Agrega a quien le vas a cobrar. Después podrás crear sus proyectos con hitos.</p>
              <Link className="btn primario" to="/clientes/nuevo">
                Agregar un cliente
              </Link>
            </>
          )}
        </div>
      ) : (
        <div className="hoja">
          {clientes.data.map((c) => (
            <RenglonCliente key={c.id} cliente={c} />
          ))}
        </div>
      )}
    </>
  )
}

function RenglonCliente({ cliente }: { cliente: Cliente }) {
  const queryClient = useQueryClient()
  const toast = useToast()
  const accion = cliente.archivadoEn ? 'restaurar' : 'archivar'
  const cambiar = useMutation({
    mutationFn: () => api<Cliente>(`/clientes/${cliente.id}/${accion}`, { method: 'POST' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clientes'] })
      toast(accion === 'archivar' ? `${cliente.nombre} quedó archivado` : `${cliente.nombre} volvió a activos`)
    },
  })
  const contacto = [cliente.empresa, cliente.email, cliente.telefono].filter(Boolean).join(' · ')

  return (
    <div className="renglon">
      <b>{cliente.nombre}</b>
      <span />
      <small>{contacto || 'Sin datos de contacto'}</small>
      <div className="acciones">
        <Link className="btn secundario" to={`/clientes/${cliente.id}`}>
          Editar
        </Link>
        <button className="btn texto" onClick={() => cambiar.mutate()} disabled={cambiar.isPending}>
          {accion === 'archivar' ? 'Archivar' : 'Restaurar'}
        </button>
        {cambiar.isError && (
          <span className="mensaje-error" role="alert">
            {mensajeDeError(cambiar.error)}
          </span>
        )}
      </div>
    </div>
  )
}

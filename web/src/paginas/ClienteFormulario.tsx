import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { api } from '../api/cliente'
import { useCliente, type Cliente, type DatosCliente } from '../api/clientes'
import { erroresDeFormulario } from '../api/errores'
import { Campo } from '../componentes/Campo'
import { ErrorCarga, Esqueleto } from '../componentes/Estados'
import { useToast } from '../componentes/toast'
import { useEnfocarError } from '../componentes/useEnfocarError'
import { reglas, useValidacion } from '../validacion'

// Crear (/clientes/nuevo) y editar (/clientes/:id) usan el mismo formulario.
export function ClienteFormulario() {
  const { id } = useParams()
  const cliente = useCliente(id)

  if (id && cliente.isPending) return <Esqueleto renglones={6} />
  if (id && cliente.isError) return <ErrorCarga error={cliente.error} onReintentar={() => cliente.refetch()} />
  return <Formulario key={id ?? 'nuevo'} cliente={cliente.data} />
}

function Formulario({ cliente }: { cliente?: Cliente }) {
  const queryClient = useQueryClient()
  const navegar = useNavigate()
  const toast = useToast()
  const [datos, setDatos] = useState<DatosCliente>({
    nombre: cliente?.nombre ?? '',
    empresa: cliente?.empresa ?? '',
    email: cliente?.email ?? '',
    telefono: cliente?.telefono ?? '',
  })

  const guardar = useMutation({
    mutationFn: () =>
      cliente
        ? api<Cliente>(`/clientes/${cliente.id}`, { method: 'PUT', body: JSON.stringify(datos) })
        : api<Cliente>('/clientes', { method: 'POST', body: JSON.stringify(datos) }),
    onSuccess: (guardado) => {
      queryClient.invalidateQueries({ queryKey: ['clientes'] })
      toast(cliente ? 'Cambios guardados' : `${guardado.nombre} quedó registrado`)
      navegar('/clientes')
    },
  })

  const errores = erroresDeFormulario(guardar.error)
  const formulario = useEnfocarError(guardar.error)
  const v = useValidacion({
    nombre: reglas.nombre(datos.nombre, 120, 'Escribe el nombre del cliente (máximo 120 caracteres).'),
    email: reglas.correoOpcional(datos.email ?? ''),
    telefono: reglas.telefonoOpcional(datos.telefono ?? ''),
  })
  const cambiar = (campo: keyof DatosCliente) => (e: { target: { value: string } }) =>
    setDatos((d) => ({ ...d, [campo]: e.target.value }))

  function enviar(e: FormEvent) {
    e.preventDefault()
    if (v.validarTodo()) guardar.mutate()
  }

  return (
    <form ref={formulario} className="hoja" onSubmit={enviar} noValidate>
      <h1 className="titulo-hoja">{cliente ? 'Editar cliente' : 'Nuevo cliente'}</h1>
      {errores.general && (
        <p className="mensaje-error" role="alert">
          {errores.general}
        </p>
      )}
      <Campo id="nombre" etiqueta="Nombre" required maxLength={120} value={datos.nombre} onChange={cambiar('nombre')} onBlur={v.alSalir('nombre')} error={v.error('nombre') ?? errores.campo('nombre')} ayuda="Como lo verá en sus enlaces. Ej.: Panadería Rosales" />
      <Campo id="empresa" etiqueta="Empresa (opcional)" maxLength={120} value={datos.empresa} onChange={cambiar('empresa')} error={errores.campo('empresa')} />
      <Campo id="email" etiqueta="Correo (opcional)" type="email" autoComplete="off" value={datos.email} onChange={cambiar('email')} onBlur={v.alSalir('email')} error={v.error('email') ?? errores.campo('email')} />
      <Campo id="telefono" etiqueta="Teléfono (opcional)" type="tel" inputMode="tel" value={datos.telefono} onChange={cambiar('telefono')} onBlur={v.alSalir('telefono')} error={v.error('telefono') ?? errores.campo('telefono')} ayuda="Para enviarle el enlace por WhatsApp." />
      <div className="botonera">
        <button className="btn primario" type="submit" disabled={guardar.isPending}>
          {guardar.isPending ? 'Guardando…' : cliente ? 'Guardar cambios' : 'Guardar cliente'}
        </button>
        <Link className="btn secundario" to="/clientes">
          Cancelar
        </Link>
      </div>
    </form>
  )
}

import { useMutation } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { api, ErrorApi, type Sesion } from '../api/cliente'
import { useSesion } from '../auth/contexto'
import { Campo } from '../componentes/Campo'
import { useEnfocarError } from '../componentes/useEnfocarError'

export function Registro() {
  const { iniciar } = useSesion()
  const navegar = useNavigate()
  const [datos, setDatos] = useState({ nombre: '', email: '', contrasena: '' })

  const registrar = useMutation({
    mutationFn: () => api<Sesion>('/auth/registro', { method: 'POST', body: JSON.stringify(datos) }),
    onSuccess: (sesion) => {
      iniciar(sesion)
      navegar('/cobros', { replace: true })
    },
  })

  const error = registrar.error instanceof ErrorApi ? registrar.error : null
  const formulario = useEnfocarError(registrar.error)
  const errorDe = (campo: string) => error?.errores[campo]?.[0]
  // Errores por campo van bajo cada campo; los demás, arriba del formulario.
  const mensajeGeneral = !registrar.error
    ? null
    : !error
      ? 'No pudimos conectar. Inténtalo de nuevo.'
      : Object.keys(error.errores).length === 0
        ? error.message
        : null
  const cambiar = (campo: keyof typeof datos) => (e: { target: { value: string } }) =>
    setDatos((d) => ({ ...d, [campo]: e.target.value }))

  function enviar(e: FormEvent) {
    e.preventDefault()
    registrar.mutate()
  }

  return (
    <main className="contenido angosto">
      <h1 className="marca-grande">Hito</h1>
      <p className="meta">Cada entrega aprobada, cada cobro claro.</p>
      <form ref={formulario} className="hoja" onSubmit={enviar} noValidate>
        <h2>Crear cuenta</h2>
        {mensajeGeneral && (
          <p className="mensaje-error" role="alert">
            {mensajeGeneral}
          </p>
        )}
        <Campo id="nombre" etiqueta="Tu nombre" autoComplete="name" required value={datos.nombre} onChange={cambiar('nombre')} error={errorDe('nombre')} />
        <Campo id="email" etiqueta="Correo" type="email" autoComplete="email" required value={datos.email} onChange={cambiar('email')} error={errorDe('email')} />
        <Campo
          id="contrasena"
          etiqueta="Contraseña"
          type="password"
          autoComplete="new-password"
          required
          value={datos.contrasena}
          onChange={cambiar('contrasena')}
          error={errorDe('contrasena')}
          ayuda="Al menos 10 caracteres. Una frase es más segura que símbolos raros."
        />
        <button className="btn primario ancho" type="submit" disabled={registrar.isPending}>
          {registrar.isPending ? 'Creando cuenta…' : 'Crear cuenta'}
        </button>
      </form>
      <p className="pie-formulario">
        ¿Ya tienes cuenta? <Link to="/entrar">Entra</Link>
      </p>
    </main>
  )
}

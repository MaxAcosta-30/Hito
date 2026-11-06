import { useMutation } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { api, ErrorApi, type Sesion } from '../api/cliente'
import { useSesion } from '../auth/contexto'
import { Campo } from '../componentes/Campo'

export function Entrar() {
  const { iniciar } = useSesion()
  const navegar = useNavigate()
  const destino = (useLocation().state as { desde?: string } | null)?.desde ?? '/cobros'
  const [email, setEmail] = useState('')
  const [contrasena, setContrasena] = useState('')

  const entrar = useMutation({
    mutationFn: () => api<Sesion>('/auth/entrar', { method: 'POST', body: JSON.stringify({ email, contrasena }) }),
    onSuccess: (sesion) => {
      iniciar(sesion)
      navegar(destino, { replace: true })
    },
  })

  function enviar(e: FormEvent) {
    e.preventDefault()
    entrar.mutate()
  }

  return (
    <main className="contenido angosto">
      <h1 className="marca-grande">Hito</h1>
      <p className="meta">Cada entrega aprobada, cada cobro claro.</p>
      <form className="hoja" onSubmit={enviar} noValidate>
        <h2>Entrar</h2>
        {entrar.error && (
          <p className="mensaje-error" role="alert">
            {entrar.error instanceof ErrorApi ? entrar.error.message : 'No pudimos conectar. Inténtalo de nuevo.'}
          </p>
        )}
        <Campo id="email" etiqueta="Correo" type="email" autoComplete="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        <Campo id="contrasena" etiqueta="Contraseña" type="password" autoComplete="current-password" required value={contrasena} onChange={(e) => setContrasena(e.target.value)} />
        <button className="btn primario ancho" type="submit" disabled={entrar.isPending}>
          {entrar.isPending ? 'Entrando…' : 'Entrar'}
        </button>
      </form>
      <p className="pie-formulario">
        ¿No tienes cuenta? <Link to="/registro">Crea una</Link>
      </p>
    </main>
  )
}

import { useSesion } from '../auth/contexto'

export function Cobros() {
  const { estado } = useSesion()
  const nombre = estado.tipo === 'autenticado' ? estado.usuario.nombre : ''
  return (
    <div className="hoja vacio">
      <h2>Hola, {nombre}</h2>
      <p className="meta">Aún no hay nada por cobrar. Aquí verás cuánto te deben cuando crees tu primer proyecto.</p>
    </div>
  )
}

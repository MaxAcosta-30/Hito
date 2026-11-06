import { ErrorApi } from './cliente'

// Separa los errores de un formulario: los de campo van bajo cada campo y el resto arriba.
export function erroresDeFormulario(error: unknown) {
  const api = error instanceof ErrorApi ? error : null
  const campo = (nombre: string) => api?.errores[nombre]?.[0]
  const general = !error
    ? null
    : !api
      ? 'No pudimos conectar. Inténtalo de nuevo.'
      : Object.keys(api.errores).length === 0
        ? api.message
        : null
  return { campo, general }
}

export function mensajeDeError(error: unknown) {
  return error instanceof ErrorApi ? error.message : 'No pudimos conectar con el servidor.'
}

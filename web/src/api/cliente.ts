export type Usuario = { id: string; nombre: string; email: string }
export type Sesion = { accessToken: string; usuario: Usuario }

export class ErrorApi extends Error {
  readonly estado: number
  readonly errores: Record<string, string[]>

  constructor(estado: number, titulo: string, errores: Record<string, string[]> = {}) {
    super(titulo)
    this.estado = estado
    this.errores = errores
  }
}

// El access token vive solo en memoria: un script inyectado no lo encuentra en localStorage.
// Al recargar la página se pierde y se recupera con la cookie httpOnly de renovación.
let accessToken: string | null = null

export function guardarToken(token: string | null) {
  accessToken = token
}

// Una sola renovación a la vez: el refresh token es de un solo uso y dos llamadas
// simultáneas con el mismo token se verían como robo y cerrarían todas las sesiones.
let renovando: Promise<Sesion | null> | null = null

export function renovar(): Promise<Sesion | null> {
  renovando ??= (async () => {
    try {
      const res = await fetch('/api/auth/renovar', { method: 'POST', credentials: 'same-origin' })
      if (!res.ok) {
        accessToken = null
        return null
      }
      const sesion = (await res.json()) as Sesion
      accessToken = sesion.accessToken
      return sesion
    } finally {
      renovando = null
    }
  })()
  return renovando
}

export async function api<T>(ruta: string, opciones: RequestInit = {}, reintentar = true): Promise<T> {
  const headers = new Headers(opciones.headers)
  if (opciones.body) headers.set('Content-Type', 'application/json')
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`)

  const res = await fetch(`/api${ruta}`, { ...opciones, headers, credentials: 'same-origin' })

  // Token vencido (dura 15 min): se renueva una vez y se repite la petición.
  if (res.status === 401 && reintentar && accessToken && (await renovar())) {
    return api<T>(ruta, opciones, false)
  }
  if (!res.ok) {
    const problema = await res.json().catch(() => ({}))
    if (res.status === 429) throw new ErrorApi(429, 'Demasiados intentos. Espera un minuto y vuelve a probar.')
    throw new ErrorApi(res.status, problema.title ?? 'Algo salió mal. Inténtalo de nuevo.', problema.errors ?? {})
  }
  return res.status === 204 ? (undefined as T) : ((await res.json()) as T)
}

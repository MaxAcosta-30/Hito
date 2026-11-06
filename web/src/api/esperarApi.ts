// Render Free se apaga tras 15 min sin tráfico. Mientras despierta puede tardar ~1 min
// o responder HTML en vez de la API, así que solo cuenta como lista la respuesta esperada.
const INTERVALO_MS = 5_000
const LIMITE_MS = 90_000

async function responde(ruta: string, signal: AbortSignal): Promise<boolean> {
  try {
    const res = await fetch(ruta, { signal, cache: 'no-store' })
    return res.ok && (await res.text()) === 'Healthy'
  } catch {
    return false
  }
}

export async function esperarApi(ruta: string, signal: AbortSignal): Promise<boolean> {
  const inicio = Date.now()
  while (!signal.aborted && Date.now() - inicio < LIMITE_MS) {
    if (await responde(ruta, signal)) return true
    await new Promise((r) => setTimeout(r, INTERVALO_MS))
  }
  return false
}

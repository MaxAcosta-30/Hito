const moneda = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN', minimumFractionDigits: 2 })
const corta = new Intl.DateTimeFormat('es-MX', { day: 'numeric', month: 'short' })
const larga = new Intl.DateTimeFormat('es-MX', { day: 'numeric', month: 'short', year: 'numeric' })

export const dinero = (monto: number) => moneda.format(monto)

// "2026-12-01" es una fecha sin hora: se arma en hora local para que no cambie de día por la zona horaria.
function aFecha(valor: string) {
  if (/^\d{4}-\d{2}-\d{2}$/.test(valor)) {
    const [a, m, d] = valor.split('-').map(Number)
    return new Date(a, m - 1, d)
  }
  return new Date(valor)
}

export const fechaCorta = (valor: string) => corta.format(aFecha(valor)).replace('.', '')
export const fechaLarga = (valor: string) => larga.format(aFecha(valor)).replace('.', '')

export function hoyIso() {
  const hoy = new Date()
  return `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`
}

// Acepta "12,000.50" o "12000.5"; devuelve NaN si no es un número.
export const leerMonto = (texto: string) => (texto.trim() === '' ? NaN : Number(texto.replace(/[,\s$]/g, '')))

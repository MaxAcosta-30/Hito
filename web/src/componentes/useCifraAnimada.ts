import { useEffect, useRef, useState } from 'react'

const reducido = () => typeof matchMedia === 'function' && matchMedia('(prefers-reduced-motion: reduce)').matches

// La cifra sube desde el valor anterior (o desde 0 la primera vez) con una curva que frena al final.
// Con movimiento reducido devuelve el valor tal cual, sin animar.
export function useCifraAnimada(valor: number, ms = 700) {
  const [mostrado, setMostrado] = useState(0)
  // Último valor pintado: si la animación se interrumpe, la siguiente sigue desde ahí, sin saltos.
  const actual = useRef(0)

  useEffect(() => {
    if (reducido()) return
    const inicio = performance.now()
    const origen = actual.current
    let id = requestAnimationFrame(function paso(t) {
      const avance = Math.min(1, (t - inicio) / ms)
      const curva = 1 - (1 - avance) ** 3
      actual.current = origen + (valor - origen) * curva
      setMostrado(actual.current)
      if (avance < 1) id = requestAnimationFrame(paso)
    })
    return () => cancelAnimationFrame(id)
  }, [valor, ms])

  return reducido() ? valor : mostrado
}

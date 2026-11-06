import { useState, type FocusEvent } from 'react'

// Validación en el navegador para avisar antes de enviar. Las reglas copian las del servidor,
// que sigue siendo la fuente de verdad y vuelve a validar todo.

type Regla = () => string | undefined

const correoValido = (valor: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(valor.trim())

export const reglas = {
  nombre: (valor: string, maximo: number, mensaje: string) => () => (valor.trim().length === 0 || valor.trim().length > maximo ? mensaje : undefined),
  correo: (valor: string) => () => (correoValido(valor) ? undefined : 'El correo no es válido.'),
  correoOpcional: (valor: string) => () => (valor.trim() === '' || correoValido(valor) ? undefined : 'El correo no es válido.'),
  contrasena: (valor: string) => () => (valor.length >= 10 ? undefined : 'Usa al menos 10 caracteres.'),
  telefonoOpcional: (valor: string) => () =>
    valor.trim() === '' || /^\+?[0-9 ()-]{7,30}$/.test(valor.trim()) ? undefined : 'Usa solo números, espacios y los signos + - ( ).',
  url: (valor: string) => () => {
    try {
      const url = new URL(valor.trim())
      if ((url.protocol === 'https:' || url.protocol === 'http:') && valor.trim().length <= 500) return undefined
    } catch {
      // Cae al mensaje de abajo.
    }
    return 'Pega la dirección de la entrega; debe empezar con https:// o http://.'
  },
}

// El error de un campo aparece al salir de él con algo escrito (no mientras escribes, ni por tabular por un campo vacío).
// Una vez marcado, se recalcula en cada render: desaparece en cuanto el valor es válido.
// Los ids de los campos deben coincidir con las claves para poder enfocar el primero con error.
export function useValidacion<C extends string>(reglasDelFormulario: Record<C, Regla>) {
  const [tocados, setTocados] = useState<Partial<Record<C, boolean>>>({})
  const campos = Object.keys(reglasDelFormulario) as C[]

  const error = (campo: C) => (tocados[campo] ? reglasDelFormulario[campo]() : undefined)

  // Solo cuando el foco pasa a otro campo o a ninguna parte. Si va a un botón o enlace (Cancelar, Guardar),
  // no se marca: el mensaje empujaría el botón hacia abajo y el clic caería en el vacío. El envío valida todo igual.
  const alSalir = (campo: C) => (e: FocusEvent<HTMLInputElement>) => {
    const destino = e.relatedTarget
    const aOtroCampo = !destino || destino instanceof HTMLInputElement || destino instanceof HTMLTextAreaElement || destino instanceof HTMLSelectElement
    if (aOtroCampo && e.target.value.trim() !== '') setTocados((t) => ({ ...t, [campo]: true }))
  }

  // Al enviar: marca todos los campos y enfoca el primero con error. Devuelve true si se puede enviar.
  function validarTodo() {
    setTocados(Object.fromEntries(campos.map((c) => [c, true])) as Record<C, boolean>)
    const primero = campos.find((c) => reglasDelFormulario[c]())
    if (primero) document.getElementById(primero)?.focus()
    return !primero
  }

  return { error, alSalir, validarTodo }
}

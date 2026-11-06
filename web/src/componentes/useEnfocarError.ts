import { useEffect, useRef } from 'react'

// Al fallar el envío, lleva el foco al primer campo con error: el teclado y el lector de pantalla llegan directo a él.
export function useEnfocarError(error: unknown) {
  const formulario = useRef<HTMLFormElement>(null)
  useEffect(() => {
    if (error) formulario.current?.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus()
  }, [error])
  return formulario
}

import { useEffect, useRef } from 'react'

// Si el servidor rechaza el envío, lleva el foco al primer campo con error: el teclado y el lector de pantalla llegan directo a él.
// Solo para errores del servidor: cada rechazo es un objeto nuevo. La validación local enfoca su campo en el mismo handler.
export function useEnfocarError(error: unknown) {
  const formulario = useRef<HTMLFormElement>(null)
  useEffect(() => {
    if (error) formulario.current?.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus()
  }, [error])
  return formulario
}

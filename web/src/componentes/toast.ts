import { createContext, useContext } from 'react'

export const ToastContexto = createContext<(mensaje: string) => void>(() => {})

export const useToast = () => useContext(ToastContexto)

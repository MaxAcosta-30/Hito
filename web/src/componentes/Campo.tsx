import type { InputHTMLAttributes, TextareaHTMLAttributes } from 'react'

type Props = InputHTMLAttributes<HTMLInputElement> & {
  id: string
  etiqueta: string
  error?: string
  ayuda?: string
}

export function Campo({ id, etiqueta, error, ayuda, ...input }: Props) {
  const descripcion = error ? `${id}-error` : ayuda ? `${id}-ayuda` : undefined
  return (
    <div className={error ? 'campo error' : 'campo'}>
      <label htmlFor={id}>{etiqueta}</label>
      <input id={id} name={id} aria-invalid={error ? true : undefined} aria-describedby={descripcion} {...input} />
      {error ? (
        <span id={`${id}-error`} className="mensaje-error">
          {error}
        </span>
      ) : (
        ayuda && (
          <span id={`${id}-ayuda`} className="ayuda">
            {ayuda}
          </span>
        )
      )}
    </div>
  )
}

type PropsArea = TextareaHTMLAttributes<HTMLTextAreaElement> & { id: string; etiqueta: string; error?: string }

export function CampoArea({ id, etiqueta, error, ...area }: PropsArea) {
  return (
    <div className={error ? 'campo error' : 'campo'}>
      <label htmlFor={id}>{etiqueta}</label>
      <textarea id={id} name={id} rows={3} aria-invalid={error ? true : undefined} aria-describedby={error ? `${id}-error` : undefined} {...area} />
      {error && (
        <span id={`${id}-error`} className="mensaje-error">
          {error}
        </span>
      )}
    </div>
  )
}

import { Link } from 'react-router'

// Describe solo lo que el código hace hoy. Si cambia lo que se guarda o dónde, este texto cambia con él.
export function Privacidad() {
  return (
    <main className="contenido angosto privacidad">
      <p className="meta">
        <Link to="/entrar">← Hito</Link>
      </p>
      <h1>Aviso de privacidad</h1>
      <p className="meta">Última actualización: 6 de noviembre de 2026</p>

      <p>
        Hito es un proyecto de portafolio de Max Acosta, sin fines comerciales. Este aviso explica qué datos guarda, para qué y cómo pedir que se borren.
      </p>

      <h2>Qué datos se guardan</h2>
      <ul>
        <li>
          <b>De tu cuenta:</b> nombre, correo y contraseña. La contraseña nunca se guarda tal cual, solo su hash.
        </li>
        <li>
          <b>De tus clientes:</b> nombre y, si los escribes, empresa, correo y teléfono. Si registras datos de un cliente, eres responsable de contar con su permiso.
        </li>
        <li>
          <b>De tus proyectos:</b> hitos, montos, fechas, enlaces de entrega, comentarios del cliente, pagos registrados y el historial de quién hizo qué y cuándo.
        </li>
        <li>
          <b>Del enlace del cliente:</b> solo una huella (hash) del enlace, no el enlace en sí.
        </li>
      </ul>

      <h2>Para qué se usan</h2>
      <p>Solo para que la aplicación funcione: mostrarte tus proyectos y cobros, y que tu cliente revise y apruebe desde su enlace. No se venden, no se comparten con terceros para publicidad y Hito no envía correos.</p>

      <h2>Cookies</h2>
      <p>
        Una sola cookie, <code translate="no">hito_renovacion</code>, para mantener tu sesión abierta. Es <code>httpOnly</code>: el código de la página no puede leerla. No hay cookies de
        analítica ni de publicidad.
      </p>

      <h2>Dónde se guardan</h2>
      <p>
        En servidores de Supabase (base de datos) y Render (API) en Oregón, Estados Unidos; la página se sirve desde Vercel. Estos proveedores pueden registrar datos técnicos, como la
        dirección IP, en sus bitácoras.
      </p>

      <h2>Cuánto tiempo</h2>
      <ul>
        <li>
          <b>Demo:</b> la cuenta de ejemplo vence a las 24 horas y se borra con todos sus datos en la siguiente limpieza.
        </li>
        <li>
          <b>Cuentas registradas:</b> hasta que pidas borrarlas. Los eventos del historial no se editan, pero se borran junto con la cuenta.
        </li>
      </ul>

      <h2>Tus derechos</h2>
      <p>
        Puedes pedir acceso, corrección o borrado de tus datos, u oponerte a su uso. Escríbele al responsable a través de{' '}
        <a href="https://github.com/MaxAcosta-30" target="_blank" rel="noopener noreferrer">
          su perfil de GitHub
        </a>
        . Como es una demo de portafolio, no uses Hito para datos sensibles ni para cobros reales.
      </p>
    </main>
  )
}

# Auditoría de cierre (2026-11-06)

Antes de guardar el proyecto, Max pidió dejar todo bien. Se auditó con herramientas, no a ojo.

**Seguridad**
- `dotnet list package --vulnerable --include-transitive` y `npm audit` (web y e2e): 0 vulnerabilidades.
- Búsqueda de secretos en todo el historial de Git (contraseñas, JWT, claves de Supabase, cadenas de conexión): nada fuera de los valores de prueba marcados como no secretos.
- Producción: CSP estricta, HSTS (`max-age=63072000; includeSubDomains; preload`), `nosniff`, `Permissions-Policy`, y `noindex` y `no-referrer` en `/p/*`. Sin CORS para otros orígenes. Errores: 400 ante JSON mal formado, 401 genérico en el inicio de sesión y 500 sin detalles internos.

**Lighthouse 13.5 en producción (celular)**

| Página | Rendimiento | Accesibilidad | Buenas prácticas | SEO |
|---|---|---|---|---|
| `/` | 100 | 96 | 100 | 92 |
| `/entrar` | 99 | 100 | 96 | 91 |
| `/privacidad` | 99 | 100 | 100 | 91 |

**Corregido a partir de la auditoría** (rama `calidad/cierre`)
- **Contraste:** en la landing oscura, el sello CAMBIOS usaba el café del modo claro; la landing ya tiene sus propios colores de estado.
- **`robots.txt`:** no existía (Vercel devolvía el HTML de la app); ahora excluye `/p/` y `/demo`.
- **401 en la consola de cada visitante anónimo:** la app ya no pide `/auth/renovar` si el navegador no tiene la marca no secreta `hito_sesion` en `localStorage` (nunca el token). Ahorra una petición y el error. Efecto único: quien tenía sesión antes de este cambio tendrá que entrar de nuevo una vez.
- Probado con Playwright: el visitante anónimo hace 0 renovaciones, la sesión sobrevive a recargar y, tras salir, no se pide nada.

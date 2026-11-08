# Auditoría de seguridad · 2026-11-08

Revisión con la lista de `software-maestro` §9 (nivel C: cuentas). Pregunta guía: ¿puede alguien generar un cobro, leer datos ajenos o robar un secreto?

## Riesgo de factura

| Servicio | Plan | Tarjeta | Peor caso ante abuso |
|---|---|---|---|
| Supabase | Free | No (confirmado por Max, etapa 1) | Se llena (500 MB) o se pausa; no cobra |
| Render | Free | No | Se agotan horas o se duerme; no cobra |
| Vercel | Hobby | No | Se pausa por límite; no cobra |
| APIs de pago (IA, correo, SMS, mapas) | Ninguna | — | No hay ninguna en el código |

**Conclusión:** sin tarjeta en ningún servicio, un abuso puede tumbar la demo pero no generar deuda. Mantener así: si algún día se agrega tarjeta, poner límites de gasto antes.

## Secretos

- Historial completo (110 commits, todas las ramas) escaneado con patrones de JWT, cadenas de conexión, claves de Supabase/Stripe/AWS/GitHub, claves privadas y deploy hooks de Render: **limpio**. Solo aparecen claves de prueba marcadas `no-es-secreta` (CI, e2e, integración) y el ejemplo `cambia-esto`.
- gitleaks no se pudo ejecutar en esta sesión (descarga bloqueada); el escaneo fue con expresiones regulares sobre `git log -p --all`.
- La API no arranca si falta `Jwt:Clave` o mide menos de 32 bytes; no hay clave por defecto en `appsettings`.
- La web no usa variables de entorno: ninguna clave llega al navegador. El access token vive en memoria; `localStorage` solo guarda una marca booleana.
- Secretos de despliegue (`SUPABASE_CONNECTION_STRING`, `RENDER_DEPLOY_HOOK_URL`) solo en el environment `produccion` de GitHub, restringido por rama. CI usa `pull_request` (no `pull_request_target`): un fork no los recibe.
- **Corregido:** `.gitignore` ignoraba `.env` pero no `.env.local` ni `.env.production` en la raíz. Ahora ignora `.env.*` salvo `.env.example`.

## Autenticación y abuso

- Identity con PBKDF2, contraseña mínima de 10, bloqueo de 5 fallos por cuenta, mismo mensaje para correo inexistente/contraseña mala/bloqueo.
- JWT HS256 de 15 min en memoria; refresh de 256 bits en cookie `httpOnly`, `Secure`, `SameSite=Strict`, `Path=/api/auth`, guardado como hash, con rotación y revocación total al detectar reutilización.
- Todas las rutas de datos exigen sesión (`RequireAuthorization` en grupos) y filtran por dueño; hay pruebas de aislamiento entre usuarios.
- Límites existentes: registro 10/min, demo 10/min, enlaces públicos 120/min (globales).
- **Corregido:** `/auth/entrar` y `/auth/renovar` no tenían límite. Ahora: 10 entradas y 30 renovaciones por minuto por IP (`Auth:EntradasPorMinuto`, `Auth:RenovacionesPorMinuto`), con pruebas de integración. La IP sale de `X-Forwarded-For`; quien llame directo a Render puede falsearla, por eso el bloqueo por cuenta sigue siendo la defensa de fondo.

## Navegador y producción (verificado en vivo)

- Web (Vercel): HSTS con preload, CSP estricta (`script-src 'self'`, `frame-ancestors 'none'`), `nosniff`, `Permissions-Policy`; `Referrer-Policy: no-referrer` en los enlaces del cliente.
- API (Render): sin CORS habilitado (solo la propia web vía rewrite); Swagger/OpenAPI no expuestos (404); rutas privadas responden 401 sin sesión; `/health/ready` solo dice `Healthy`; errores 500 sin detalles internos.

## Dependencias

- `dotnet list package --vulnerable --include-transitive`: 0 en los 6 proyectos.
- `npm audit` en `web/`: 0 (con y sin dependencias de desarrollo).

## Pendiente que solo Max puede verificar (NO VERIFICADO)

1. **Data API de Supabase apagada.** El documento de despliegue dice que sí, pero el conector de esta sesión está en otra cuenta. Verificar en Supabase → Project Settings → Data API que esté desactivada. Si estuviera encendida, cualquiera con la URL y la clave anónima podría leer las tablas.
2. **`Jwt__Clave` en Render es aleatoria** (64+ caracteres generados, p. ej. `openssl rand -base64 48`) y no una de las claves de prueba del repo.
3. **Ningún servicio tiene tarjeta** (revisión trimestral, ver mantenimiento).

## Riesgos aceptados (demo de portafolio, sin costo)

- Un abuso sostenido de `/demo` (10/min) podría acercar la base al límite de 500 MB en días; Supabase pasaría a solo lectura. Las demos expiradas se borran al crear una nueva.
- Los refresh tokens expirados no se purgan; crecen lento.
- `/auth/entrar` responde un poco más rápido si el correo no existe (no calcula hash): permite inferir cuentas por tiempo. Bajo impacto en una demo.

## Mantenimiento

- Mensual: alertas de Dependabot y `npm audit`.
- Trimestral: confirmar que no hay tarjeta en Supabase, Render y Vercel; revisar accesos y secretos de GitHub.
- Si una clave se filtra: rotar `Jwt__Clave` en Render (cierra todas las sesiones) y la contraseña de la base en Supabase (actualizar el secreto de GitHub y la variable de Render).

# Etapa 5 · Calidad y Kubernetes

**E2E con Playwright:** rama `etapa-5/e2e`.
- `tests/e2e/flujo-de-cobro.spec.ts`: registro → cliente → proyecto con 2 hitos ($15,000 calculado en el servidor) → enviar → el cliente acepta desde su enlace → entregar → el cliente aprueba → pagar → el panel muestra $5,000 por cobrar.
- Freelancer y cliente usan contextos separados: el cliente no tiene cuenta ni cookie. Corre en Chromium con la pantalla de un Pixel 7, porque la vista del cliente se diseñó primero para móvil.
- La pila es real: Vite (proxy `/api`) → API .NET → PostgreSQL desechable. Las migraciones se aplican antes de la prueba, igual que en producción; la API no migra al arrancar.
- CI: job `e2e` con PostgreSQL como servicio. `deploy` ahora espera a `build-test` y a `e2e`. Si falla, el reporte se sube como artefacto por 7 días.
- Hallazgo: todos los `<dialog>` compartían `id="dialogo-titulo"`, así que el lector de pantalla anunciaba "Registrar pago" en el diálogo de entrega. Se corrigió con `useId`.
- Probado en local: 3 de 3 corridas en verde (`--repeat-each 3`).

**Prueba en Kubernetes (kind):** rama `etapa-5/kind`. ADR 0006.
- `k8s/postgres.yaml` y `k8s/api.yaml` (2 réplicas, sondas live/ready, sin root, solo lectura). `k8s/probar.sh` crea secretos aleatorios, migra con el bundle y prueba `/health/ready` y `POST /demo` → `GET /cobros` ($76,000).
- CI: job `kind` entre `build-test` y `deploy`. `deploy` aplica en Supabase el mismo bundle que se probó en kind.
- Hallazgo: la API no arrancaba por el límite de inotify del nodo; se desactivó la recarga de `appsettings.json` en el contenedor.
- Probado en local con kind v0.33.0 sobre Podman sin root: `Prueba en kind: OK`.

**Revisión de accesibilidad, rendimiento y seguridad:** rama `etapa-5/revision`.
- Accesibilidad (Web Interface Guidelines de Vercel): pestañas del proyecto con el patrón ARIA completo (tabpanel, flechas, Inicio/Fin) y en la URL (`?pestana=historial`); foco en el primer campo con error al enviar; confirmación antes de revocar o reemplazar el enlace del cliente; "Saltar al contenido"; un `h1` por formulario; esqueletos anunciados; `theme-color`; correos y URLs sin corrector. Probado con una prueba temporal de Playwright (teclado, URL tras recargar, foco en error, enlace de saltar), que después se borró.
- Rendimiento: un solo bundle de 103 kB gzip; las fuentes de `@fontsource` usan `unicode-range`, así que el navegador solo baja el subconjunto latino. No hay listas largas ni lecturas de layout en render. **No se midió con Lighthouse**: no se hicieron cambios de rendimiento.
- Seguridad: `vercel.json` agrega CSP estricta (`script-src 'self'`, `frame-ancestors 'none'`), `nosniff` y `Permissions-Policy`. Probado contra la build de producción con la CSP inyectada: cero violaciones (y la prueba sí detecta una CSP rota). **Sin probar en Vercel** hasta fusionar.
- Revisado sin cambios: filtro global por `OwnerId`, `IgnoreQueryFilters()` solo tras validar el hash del token, bloqueo de cuenta tras 5 intentos, cookie `httpOnly`/`Secure`/`Strict`, 500 sin detalles internos, imagen chiseled sin root.

- `code-review` (medium) sobre todo el diff de la etapa: 8 hallazgos; 6 corregidos (sonda de PostgreSQL por TCP, el foco ya no salta a otro campo mientras escribes, el esqueleto sin `aria-busy`, comentario de "atrás" corregido, `probar.sh` se puede repetir en el mismo clúster). Se dejaron 2: el port-forward ya queda cubierto con la sonda corregida, y `kind` va después de `build-test` porque así lo pide CLAUDE.md.
- `probar.sh` probado en local 2 veces seguidas sobre el mismo clúster: OK y OK.
- En una corrida de CI, `playwright install --with-deps` tardó 9 min por apt (la prueba en sí, 6.5 s). Si se repite, cachear los navegadores.

**Riesgos conocidos que no se corrigieron** (decisión de Max):
- Los límites de `/publico`, `/demo` y `/auth/registro` son globales, no por IP: alguien puede agotarlos y bloquear a los demás un minuto. Particionar por IP exige confiar en `X-Forwarded-For` a través de Vercel y Render, y eso no se ha verificado.
- El bloqueo de cuenta permite que un tercero bloquee una cuenta ajena 5 minutos escribiendo mal su contraseña.
- `/auth/registro` revela si un correo ya tiene cuenta (error de correo duplicado).
- La CSP bloquea la barra de comentarios de Vercel en los previews; producción no la usa.

## Cómo correr el e2e en local

```bash
podman run -d --rm --name hito-e2e-db -e POSTGRES_DB=hito_e2e -e POSTGRES_USER=hito -e POSTGRES_PASSWORD=e2e -p 5433:5432 docker.io/library/postgres:17-alpine
export E2E_CONNECTION_STRING="Host=localhost;Port=5433;Database=hito_e2e;Username=hito;Password=e2e"
ConnectionStrings__Hito="$E2E_CONNECTION_STRING" Jwt__Clave=solo-para-tiempo-de-diseno-no-es-secreta-000 \
  dotnet ef database update -p src/Hito.Infrastructure -s src/Hito.Api
cd tests/e2e && npm ci && npx playwright install chromium && npx playwright test
```

Playwright levanta la API y Vite por su cuenta; en local reutiliza los que ya estén corriendo.

## Cómo correr la prueba en kind en local

```bash
docker build -t hito-api:ci .          # con Podman: etiquetarla como docker.io/library/hito-api:ci
kind create cluster --name hito
kind load docker-image hito-api:ci --name hito
dotnet ef migrations bundle -p src/Hito.Infrastructure -s src/Hito.Api -o efbundle   # con Jwt__Clave y ConnectionStrings__Hito de diseño
k8s/probar.sh ./efbundle
kind delete cluster --name hito
```

## Estado de la Etapa 5

E2E, prueba en kind y revisión terminados (PR #23, #24 y #25, encadenados). CI en verde en los tres: `build-test`, `web`, `e2e` y `kind`. Pendiente:
- Aprobación de Max y fusión en orden (#22 → #23 → #24 → #25, con merge commit).
- Tras fusionar, en `main`: comprobar que `deploy` descarga el bundle de `kind` y lo aplica en Supabase, y revisar las cabeceras en producción con `curl -sI https://<dominio>/entrar`. El preview de Vercel pide inicio de sesión y no se pudo revisar.
- Las respuestas de entrevista de las Etapas 3, 4 y 5.

## Preguntas de entrevista pendientes

**E2E**
1. ¿Por qué el cliente corre en un contexto de navegador distinto al del freelancer? ¿Qué dejaría de probar si compartieran contexto?
2. ¿Por qué el e2e usa la API y PostgreSQL reales en lugar de simular la API? ¿Qué cuesta esa decisión?
3. El e2e encontró que todos los diálogos compartían un id. ¿Qué escuchaba un usuario de lector de pantalla y cómo lo resuelve `useId`?

**Kubernetes (kind)**
1. ¿Por qué `/health/live` no revisa la base y `/health/ready` sí? ¿Qué pasaría en Kubernetes si las dos la revisaran?
2. ¿Por qué el deploy aplica el mismo bundle que se probó en kind en vez de generar otro? ¿Qué no garantiza esto sobre la imagen de Render?
3. Con 2 réplicas, el token de `POST /demo` puede validarse en otro pod. ¿Por qué funciona sin compartir sesión entre pods?

**Revisión**
1. ¿Qué ataque frena `frame-ancestors 'none'` y por qué la CSP puede ser `script-src 'self'` sin `'unsafe-inline'`?
2. Los límites de `/publico` y `/demo` son globales. ¿Cómo los aprovecharía un atacante y por qué particionar por IP detrás de Vercel y Render no es trivial?
3. ¿Por qué un `role="tab"` sin flechas ni `tabIndex` es peor que unos botones normales?

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

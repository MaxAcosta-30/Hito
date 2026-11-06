# 0006 · Prueba en kind con manifiestos simples y un solo bundle de migraciones

**Estado:** aceptada · 2026-11-06

**Contexto:** el despliegue real es Render + Supabase, sin Kubernetes. Se quiere demostrar que la API corre en Kubernetes con sondas correctas, sin pagar un clúster y sin presentarlo como producción.

**Decisión:**
- En CI, `helm/kind-action` crea un clúster `kind`. Se carga la imagen construida con el mismo `Dockerfile` que usa Render (`kind load`, sin registro).
- `k8s/` tiene dos manifiestos: PostgreSQL desechable (`emptyDir`) y la API con 2 réplicas, `livenessProbe` en `/health/live` y `readinessProbe` en `/health/ready`, usuario sin root, sistema de archivos de solo lectura y sin capabilities.
- Los secretos se crean en cada corrida con `openssl rand` y mueren con el clúster.
- `k8s/probar.sh` aplica las migraciones con el bundle, espera el rollout y prueba `/health/ready` y `POST /demo` → `GET /cobros` a través del Service. Con 2 réplicas, el token emitido por un pod puede validarse en otro.
- El bundle se genera una vez en el job `kind` y `deploy` aplica ese mismo archivo en Supabase: lo que llega a la base es lo que se probó.
- Orden: `build-test` → `kind` → `deploy` (migraciones → deploy hook). `deploy` también espera a `e2e`.

**Por qué no Helm ni Kustomize:** dos Deployments sin variantes por entorno no justifican plantillas.
**Por qué no un Job de Kubernetes para migrar:** necesitaría otra imagen con el bundle. Un `port-forward` desde el runner prueba el mismo binario que se usa contra Supabase.

**Hallazgo:** en un nodo con muchos contenedores, la API no arrancaba porque `appsettings.json` se vigila con inotify y el límite del host (128) se agotó. En el contenedor la configuración no cambia, así que se desactivó con `DOTNET_hostBuilder__reloadConfigOnChange=false`.

**Límite:** la imagen que corre en Render la construye Render; no es el mismo archivo que se probó en kind, solo el mismo `Dockerfile`.

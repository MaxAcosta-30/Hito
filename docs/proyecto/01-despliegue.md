# Etapa 1 · Despliegue temprano

**Estado:** en curso (2026-11-05). Cuentas de Supabase, Render y Vercel creadas por Max, sin tarjeta.

- Supabase en Oregon (West US), con la Data API desactivada. Render también va en Oregon para quedar junto a la base.
- `Dockerfile` (imagen chiseled, sin root, 171 MB) probado en local con `/health/live` y `/health/ready` en 200.
- Primera migración `Inicial` vacía: sirve para probar el bundle. Aplicada en local leyendo `ConnectionStrings__Hito`.
- `ci.yml`: build-test → deploy (solo en push a `main`, environment `produccion`): bundle → migraciones → deploy hook. En `main` no se cancela un despliegue en curso.
- `keepalive.yml`: lunes y jueves llama a `/health/ready` (variable `API_URL`) hasta recibir `Healthy`.
- Pendiente: servicio en Render, secretos, página mínima en Vercel con rewrite y confirmar que no hay tarjeta.

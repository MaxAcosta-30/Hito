# Etapa 1 · Despliegue temprano

**Estado:** aprobada por Max (2026-11-05). Max confirmó que Supabase, Render y Vercel no tienen tarjeta registrada.

- Frontend: https://hito-one.vercel.app (Vercel Hobby, Root Directory `web`). El rewrite `/api/*` → Render está probado: `live` y `ready` en 200.
- API: https://hito-api-a4su.onrender.com (Render Free, Oregon, Docker, Auto-Deploy desactivado, health check `/health/live`).
- Base: Supabase Free en Oregon, session pooler `aws-1-us-west-2.pooler.supabase.com:5432` con `Maximum Pool Size=5` y la Data API desactivada.
- CI en `main`: build-test + web → deploy (bundle → migraciones → deploy hook), en verde. `keepalive` lunes y jueves, probado a mano.
- Secretos en el environment `produccion` (solo `main`): `SUPABASE_CONNECTION_STRING` y `RENDER_DEPLOY_HOOK_URL`. Variable del repo: `API_URL`.
- Sin probar: el aviso "Despertando la demo…" en un navegador durante un arranque en frío real.

**Siguiente paso:** Etapa 2 (diseño) en una sesión nueva.

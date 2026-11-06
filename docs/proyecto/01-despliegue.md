# Etapa 1 · Despliegue temprano

**Estado:** en curso (2026-11-05). Cuentas de Supabase, Render y Vercel creadas por Max, sin tarjeta.

- Supabase en Oregon (West US), con la Data API desactivada. Render también va en Oregon para quedar junto a la base.
- `Dockerfile` (imagen chiseled, sin root, 171 MB) probado en local con `/health/live` y `/health/ready` en 200.
- Primera migración `Inicial` vacía: sirve para probar el bundle. Aplicada en local leyendo `ConnectionStrings__Hito`.
- `ci.yml`: build-test → deploy (solo en push a `main`, environment `produccion`): bundle → migraciones → deploy hook. En `main` no se cancela un despliegue en curso.
- `keepalive.yml`: lunes y jueves llama a `/health/ready` (variable `API_URL`) hasta recibir `Healthy`.
- API en Render (Oregon, Free): https://hito-api-a4su.onrender.com. El pipeline completo (pruebas → migraciones en Supabase → deploy hook) está en verde. `keepalive` probado a mano.
- Supabase usa el host `aws-1-us-west-2.pooler.supabase.com`. A `API_URL` se le había colado un `\r\n` al pegarla.
- `web/`: Vite + React + TS (dependencias publicadas antes del 22-oct). Espera a que `/api/health/live` responda `Healthy` y muestra el aviso de arranque. `vercel.json` hace el rewrite de `/api/*` → Render.
- Pendiente: proyecto en Vercel (Root Directory `web`), probar la página publicada y confirmar que no hay tarjeta.

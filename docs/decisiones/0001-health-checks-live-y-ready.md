# 0001 · Health checks separados: live y ready

**Estado:** aceptada · 2026-11-05

**Contexto:** la API corre en Render Free (se apaga tras 15 min sin tráfico) y Supabase Free se pausa tras una semana con poca actividad. Si el health check de Render dependiera de la base, una base pausada provocaría reinicios sin motivo.

**Decisión:**
- `/health/live` no revisa dependencias: solo dice que el proceso responde. Es el health check de Render y lo que la interfaz consulta para saber si la API ya despertó.
- `/health/ready` revisa PostgreSQL con `AddDbContextCheck` (paquete oficial de Microsoft). Lo usan la prueba de `kind` y el flujo programado que mantiene activa a Supabase.
- EF Core usa `EnableRetryOnFailure` para tolerar cortes breves de la base.

**Por qué no un solo `/health`:** mezclar ambos haría que una base pausada reiniciara la API en bucle.
**Por qué no `AspNetCore.HealthChecks.NpgSql`:** es de terceros y el `DbContext` ya existe; no aporta nada hoy.

**Timeout del check:** con `EnableRetryOnFailure`, `/health/ready` tardaba 58 s en responder 503 con la base caída. El check de la base tiene un timeout de 5 s; las consultas normales conservan los reintentos.

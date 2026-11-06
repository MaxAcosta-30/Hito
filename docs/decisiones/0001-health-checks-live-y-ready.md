# 0001 · Health checks separados: live y ready

**Estado:** aceptada · 2026-11-05

**Contexto:** la API corre en Container Apps con `min-replicas 0` y Azure SQL se pausa por inactividad. Si un probe falla por la base, el orquestador podría reiniciar el contenedor sin motivo.

**Decisión:**
- `/health/live` no revisa dependencias: solo dice que el proceso responde. Se usa como liveness.
- `/health/ready` revisa SQL Server con `AddDbContextCheck` (paquete oficial de Microsoft). Se usa como readiness y en la prueba de `kind`.
- EF Core usa `EnableRetryOnFailure` para tolerar el despertar de la base.

**Por qué no un solo `/health`:** mezclar ambos haría que una base pausada reiniciara la API en bucle.
**Por qué no `AspNetCore.HealthChecks.SqlServer`:** es de terceros y el `DbContext` ya existe; no aporta nada hoy.

**Consecuencia a vigilar (Etapa 1):** un readiness probe frecuente contra la base puede impedir que Azure SQL se pause mientras haya una réplica viva. Revisar el intervalo del probe al desplegar.

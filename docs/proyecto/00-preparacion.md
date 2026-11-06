# Etapa 0 · Preparación

**Estado:** aprobada (2026-11-05). Cambio de infraestructura a Supabase/Render/Vercel en la rama `infra/supabase-render`, pendiente de aprobación (ADR 0002).

- Solución `Hito.slnx` (.NET 10 fijado con `global.json`), 4 capas, `compose.yaml` con PostgreSQL 17 y endpoints `/health/live` y `/health/ready`.
- 3 pruebas de integración con Testcontainers (PostgreSQL), en verde en local; CI en GitHub Actions en `main`.
- Referencias: de SafeLedger (MIT, propio) se toman las capas, `IEntityTypeConfiguration` y la precisión 18,2. De FreelanceGuard no se toma nada.
- .NET 10 está en `~/.dotnet`, fuera del PATH. Azure CLI ya no hace falta.
- Correr en local: `cp .env.example .env` → `docker compose up -d --wait` → `dotnet user-secrets --project src/Hito.Api set ConnectionStrings:Hito "Host=localhost;Port=5432;Database=hito;Username=hito;Password=<.env>;Maximum Pool Size=5"` → `dotnet run --project src/Hito.Api`.

**Siguiente paso:** aprobar el cambio de infraestructura y empezar la Etapa 1 (cuentas sin tarjeta y confirmar que Render Free acepta Dockerfile).

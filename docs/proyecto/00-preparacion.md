# Etapa 0 · Preparación

**Estado:** terminada, pendiente de aprobación de Max (2026-11-05).

- Solución `Hito.slnx` (.NET 10, fijado con `global.json`), 4 capas, `compose.yaml` con SQL Server 2022, `/health/live` y `/health/ready`.
- 3 pruebas de integración (Testcontainers) en verde localmente con podman; CI en `.github/workflows/ci.yml` aún no ejecutado en GitHub.
- Referencias: de SafeLedger (MIT, propio) solo la idea de capas, `IEntityTypeConfiguration` y `decimal(18,2)`; nada de FreelanceGuard.
- Faltan en la máquina: Azure CLI (Etapa 1), kind/kubectl (opcional, Etapa 5). .NET 10 está en `~/.dotnet` fuera del PATH.
- Correr local: `cp .env.example .env`, `docker compose up -d --wait`, `dotnet user-secrets --project src/Hito.Api set ConnectionStrings:Hito "Server=localhost,1433;Database=Hito;User Id=sa;Password=<.env>;TrustServerCertificate=True"`, `dotnet run --project src/Hito.Api`.
- `/health/ready` da 503 con `Database=Hito` hasta que exista la primera migración (Etapa 3).

**Siguiente paso:** crear repo en GitHub, ver CI en verde y pasar a la Etapa 1.

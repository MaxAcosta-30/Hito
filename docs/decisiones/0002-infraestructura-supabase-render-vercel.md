# 0002 · Infraestructura: Supabase, Render y Vercel en lugar de Azure

**Estado:** aceptada · 2026-11-05 · reemplaza el plan de Azure de CLAUDE.md

**Contexto:** el presupuesto es $0 y Max decidió no registrar tarjeta en ningún servicio. No se llegó a crear ningún recurso en Azure.

**Decisión:**
- PostgreSQL en Supabase Free, **solo como base de datos**, por el session pooler (puerto 5432) y con `Maximum Pool Size` bajo. Sin Supabase Auth ni API automática.
- EF Core Code-First con Npgsql; dinero en `numeric(18,2)` y `decimal`.
- API .NET 10 en Render Free con Dockerfile; frontend en Vercel Hobby con rewrite de `/api/*`.
- Despliegue con GitHub Actions: pruebas → `kind` → bundle de migraciones contra Supabase → deploy hook de Render. Secretos en GitHub Actions y Render.

**Por qué no Azure for Students:** depende de la verificación de estudiante y de un crédito que vence en 12 meses.
**Por qué no SQL Server:** no hay un SQL Server gestionado gratis y sin tarjeta; PostgreSQL con EF Core muestra el mismo Code-First.

**Costos aceptados:** arranque en frío de Render (hasta 1 min), pausa de Supabase por inactividad, sin respaldos automáticos. Es una demo de portafolio, no producción. El proyecto no demuestra Azure ni AWS.

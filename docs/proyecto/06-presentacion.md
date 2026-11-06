# Etapa 6 · Presentación

**README:** rama `etapa-6/presentacion`. Problema, demo, capturas, arquitectura (diagramas Mermaid), decisiones con enlace a cada ADR, pruebas, cómo correrlo y límites conocidos.
- Capturas en `docs/capturas/`, tomadas con Playwright sobre la build de producción en local con los datos de la demo (390 px, tema claro).
- Cifras verificadas el 2026-11-06 con `dotnet test`: 51 unitarias y 50 de integración en verde.
- Pendiente: el repositorio no tiene `LICENSE`. Es decisión de Max (por ejemplo MIT, o ninguna para conservar todos los derechos).

## Viñetas para el CV

Solo dicen lo que el repositorio demuestra.

1. Construí y publiqué **Hito**, una aplicación full stack con **ASP.NET Core 10, React 19 con TypeScript y PostgreSQL** para que un freelancer acuerde hitos con su cliente y controle sus cobros. Las reglas de estado viven en el dominio y responden 409 ante transiciones inválidas, y el aislamiento entre usuarios está verificado con pruebas de integración.
2. Implementé la autenticación con **ASP.NET Core Identity y JWT** (access token en memoria y refresh token rotado en cookie `httpOnly`), además de enlaces privados para clientes sin cuenta: token de 256 bits guardado solo como hash SHA-256, que expira y se puede revocar.
3. Automaticé **CI/CD con GitHub Actions**: 51 pruebas unitarias, 50 de integración con Testcontainers, un e2e con Playwright y una prueba de despliegue en **Kubernetes con kind dentro de CI**. Después se aplican las migraciones de EF Core (bundle) y se despliega en Render y Vercel a costo $0.

## Entrevista simulada

Las 10 preguntas se le hacen a Max en la sesión; sus respuestas y correcciones se registran aquí.

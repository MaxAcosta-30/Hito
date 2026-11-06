# Etapa 3 · Casos 1 a 5

**Caso 1 · Registro e inicio de sesión:** terminado y desplegado (2026-11-05, PR #11). Decisiones en ADR 0003.
- API: `/auth/registro`, `/entrar`, `/renovar`, `/salir` y `/yo`. Migración `AgregarIdentidad`. 12 pruebas de integración en verde (9 de auth).
- Web: `/entrar`, `/registro` y `/cobros` (protegida). La sesión se recupera con la cookie y una sola renovación a la vez.
- Probado en local a través del proxy de Vite: registro → renovar → yo → salir → renovar 401.
- Criterio de aislamiento: el mecanismo está listo (`IUsuarioActual` + filtro por dueño); la prueba "B no ve los datos de A" se completa en el caso 2 (opción A, aprobada por Max).
- Max lo probó en producción: crear cuenta, recargar sin perder la sesión, salir y volver a entrar. `Jwt__Clave` está en Render.
- Entrevista: Max falló las 3 preguntas (tokens en memoria vs cookie, rotación, SHA-256 vs hash lento). Repasarlas al inicio del caso 2.

**Siguiente paso:** caso 2 (clientes) en una sesión nueva.

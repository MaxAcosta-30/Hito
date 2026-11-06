# Etapa 3 · Casos 1 a 5

**Caso 1 · Registro e inicio de sesión:** construido en la rama `caso-1/auth`, pendiente de aprobación (2026-11-05). Decisiones en ADR 0003.
- API: `/auth/registro`, `/entrar`, `/renovar`, `/salir` y `/yo`. Migración `AgregarIdentidad`. 12 pruebas de integración en verde (9 de auth).
- Web: `/entrar`, `/registro` y `/cobros` (protegida). La sesión se recupera con la cookie y una sola renovación a la vez.
- Probado en local a través del proxy de Vite: registro → renovar → yo → salir → renovar 401.
- Criterio de aislamiento: el mecanismo está listo (`IUsuarioActual` + filtro por dueño); la prueba "B no ve los datos de A" se completa en el caso 2 (opción A, aprobada por Max).
- Sin probar: los formularios en un navegador real (Firefox sin interfaz solo captura el estado "Despertando").
- Antes de unir: Max agrega `Jwt__Clave` en Render.

**Siguiente paso:** caso 2 (clientes) en una sesión nueva.

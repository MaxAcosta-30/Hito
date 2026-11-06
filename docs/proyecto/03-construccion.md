# Etapa 3 · Casos 1 a 5

**Caso 1 · Registro e inicio de sesión:** terminado y desplegado (2026-11-05, PR #11). Decisiones en ADR 0003.
- API: `/auth/registro`, `/entrar`, `/renovar`, `/salir` y `/yo`. Migración `AgregarIdentidad`. 12 pruebas de integración en verde (9 de auth).
- Web: `/entrar`, `/registro` y `/cobros` (protegida). La sesión se recupera con la cookie y una sola renovación a la vez.
- Probado en local a través del proxy de Vite: registro → renovar → yo → salir → renovar 401.
- Criterio de aislamiento: completado en el caso 2 con la prueba `Un_usuario_no_puede_leer_ni_modificar_los_clientes_de_otro`.
- Max lo probó en producción: crear cuenta, recargar sin perder la sesión, salir y volver a entrar. `Jwt__Clave` está en Render.
- Entrevista: Max falló las 3 preguntas (tokens en memoria vs cookie, rotación, SHA-256 vs hash lento). El repaso se hizo el 2026-11-06; **las 3 preguntas nuevas siguen sin respuesta**.

**Caso 2 · Administrar clientes:** terminado en la rama `caso-2/clientes` (2026-11-06), **sin subir ni desplegar** (ver "Trabajo autónomo"). ADR 0004.
- API: `GET/POST /clientes`, `GET/PUT /clientes/{id}`, `POST /clientes/{id}/archivar` y `/restaurar`. Migración `AgregarClientes`.
- Se archiva con fecha en lugar de borrar: los proyectos y pagos del cliente conservan su historial.
- Validación: nombre obligatorio, empresa opcional, correo y teléfono opcionales con formato. 400 con el error en su campo.
- 4 pruebas de integración nuevas (16 en total), incluida la de aislamiento: B recibe 404 al leer, editar o archivar un cliente de A, y los datos de A no cambian.
- Web: `/clientes` (activos y archivados), `/clientes/nuevo` y `/clientes/:id`, con esqueleto, vacío, error con "Reintentar" y toast.
- Probado en local con Playwright (Chromium, 390 px y 1280 px, claro y oscuro): crear con validación → editar → archivar → restaurar.

**Caso 3 · Proyecto con hitos:** terminado en la rama `caso-3/proyectos` (2026-11-06), sin subir ni desplegar. ADR 0005.
- API: `GET/POST /proyectos`, `GET /proyectos/{id}`, `PUT /proyectos/{id}/alcance` y `POST /proyectos/{id}/cancelar`. Migración `AgregarProyectos`.
- Cada hito tiene título, monto en MXN (`numeric(18,2)`), fecha y entregable. El total lo calcula el dominio; un `total` enviado por el navegador se ignora.
- 409 para reglas de negocio y choques de concurrencia (`ManejadorDeReglas`).
- Nuevo `tests/Hito.UnitTests`: 12 pruebas del agregado. Integración: 6 nuevas (22 en total), incluido el aislamiento de proyectos.
- Web: `/proyectos`, `/proyectos/nuevo`, `/proyectos/:id` (pestañas Hitos y Versiones) y `/proyectos/:id/alcance`. Probado con Playwright en 390 px.

## Preguntas de entrevista pendientes

**Caso 2**
1. ¿Por qué un cliente de otro usuario responde 404 y no 403? ¿Dónde se aplica ese filtro y qué pasaría si se te olvida en una consulta nueva?
2. ¿Por qué se archiva en lugar de borrar? ¿Qué problema aparecería con los pagos si borraras clientes?
3. El `OwnerId` sale del token y no del cuerpo de la petición. ¿Qué ataque evita eso?

**Caso 3**
1. ¿Por qué el hito y sus términos son entidades distintas? ¿Qué pasaría con los pagos si los hitos vivieran dentro de cada versión?
2. ¿Por qué `decimal` y `numeric(18,2)` y no `double`? Da un ejemplo de error con `double`.
3. ¿Qué hace `xmin` en este proyecto y qué ve el usuario si dos pestañas editan el mismo proyecto a la vez?

# Etapa 4 · Casos 6 a 10

**Caso 6 · Entregar y aprobar un hito:** terminado y desplegado (2026-11-06, rama `caso-6/entregas`).
- Transiciones en `HitoDeProyecto`, siempre a través de `Proyecto`: exige alcance aceptado y que el hito esté en la versión vigente. Inválida → 409.
- API: `POST /proyectos/{id}/hitos/{hitoId}/entregar` con `evidenciaUrl` (solo http/https, máx. 500); `POST /publico/{token}/hitos/{hitoId}/aprobar` y `/cambios` con comentario obligatorio. Migración `AgregarEntregas`.
- Pruebas: 12 unitarias (todas las transiciones inválidas, URLs `javascript:`/`data:`/`ftp:` y las reglas del caso 5 con hitos avanzados) y 4 de integración. Total: 35 + 35.
- Web: "Marcar como entregado" con `<dialog>` nativo; el cliente ve la entrega, aprueba o pide cambios (comentario validado antes de enviar). Probado con Playwright.
- Desvío del diseño: si hay varias entregas, cada una tiene sus botones en su renglón en lugar de una sola barra fija.

**Caso 7 · Registrar un pago manual:** terminado y desplegado (2026-11-06, rama `caso-7/pagos`).
- `Pago` vive en el agregado `Proyecto`: solo un hito aprobado, monto > 0 con 2 decimales y nunca más de lo pendiente. Se permiten pagos parciales.
- El hito pasa a `Pagado` al cubrirse; con todos los hitos vigentes pagados, el proyecto pasa a `Completado`. Un cancelado conserva sus pagos.
- Dos pagos simultáneos: `xmin` hace que uno responda 409 (prueba de integración con dos peticiones en paralelo).
- API: `POST /proyectos/{id}/hitos/{hitoId}/pagos`. Migración `AgregarPagos`. Pruebas: 8 unitarias y 5 de integración. Total: 43 + 40.
- Web: diálogo "Registrar pago" con el pendiente prellenado y validación al instante; totales Pagado, Por cobrar y Total. Probado con Playwright.

**Caso 8 · Panel de cobros:** terminado y desplegado (2026-11-06, rama `caso-8/panel`).
- `ResumenDeCobros.Calcular` (dominio, función pura): por cobrar, aprobado sin pagar y vencido sobre lo pendiente de la versión vigente de proyectos aceptados. "Hoy" en `America/Mexico_City`.
- API: `GET /cobros`. Pruebas: 5 unitarias y 2 de integración (los totales coinciden con los datos; aislamiento). Total: 48 + 42.
- Web: cifra "Te deben", desglose y "Necesita tu atención", con esqueleto, vacío y error. Probado con Playwright.

**Caso 9 · Historial:** terminado y desplegado (2026-11-06, PR #20).
- Cada método de `Proyecto` genera un `EventoDeProyecto` (actor, tipo, descripción en texto y fecha); `HitoDbContext` lo guarda al hacer `SaveChanges` y lanza error si un evento se modifica o se borra.
- API: solo `GET /proyectos/{id}/eventos`; PUT/PATCH/DELETE/POST responden 405. Columna identidad `Secuencia` para el orden. Migración `AgregarHistorial`.
- Pruebas: 3 unitarias y 4 de integración (incluida la de que ni el propio servidor puede editar o borrar). Total: 51 + 46.
- Web: pestaña Historial de solo lectura. Probado con Playwright.

**Caso 10 · Probar la demo:** terminado y desplegado (2026-11-06, PR #21).
- `POST /demo` crea un usuario con `DemoExpiraEn` (24 h), siembra 3 clientes y 3 proyectos con los métodos del dominio y abre su sesión. Límite global: 10 por minuto.
- Cada demo es un usuario distinto: el filtro por dueño la aísla. Cada `POST /demo` borra antes las demos vencidas en cascada.
- Hallazgo: con `NO ACTION`, ese borrado fallaba (PostgreSQL revisa Términos → Hito antes de la cascada por Versiones). Términos y Pagos → Hito pasaron a `CASCADE` (migración `CascadaDeHitos`, ADR 0005 actualizado).
- Pruebas: 4 de integración (datos coherentes: $76,000 por cobrar, $12,000 aprobado y $27,000 vencido; aislamiento; borrado en cascada de una demo vencida; límite 429). Total: 51 + 50.
- Web: "Probar la demo" en `/entrar` y aviso de demo arriba. Probado con Playwright en dos contextos.

**Estado de la Etapa 4:** casos 6 a 10 terminados y desplegados (PR #17 a #21). Max probó la demo en producción el 2026-11-06 y funciona. Pendiente: sus respuestas de entrevista y la prueba tras un arranque en frío de Render.

**Siguiente paso:** Etapa 5 (calidad y Kubernetes) en una sesión nueva: e2e con Playwright, prueba en `kind` entre las pruebas y las migraciones, y revisión de accesibilidad, rendimiento y seguridad.

## Preguntas de entrevista pendientes

**Caso 6**
1. ¿Por qué las transiciones se hacen a través de `Proyecto` y no directamente sobre el hito?
2. ¿Qué ataque evita aceptar solo `http` y `https` en la evidencia?
3. ¿Por qué el comentario se valida en la web **y** en el servidor?

**Caso 7**
1. Dos pestañas registran el pago completo a la vez. Explica paso a paso por qué solo una lo logra.
2. ¿Por qué la regla de "no pagar de más" vive en el dominio y no solo en la web?
3. ¿Qué cambiarías si el cliente pudiera pagar con tarjeta? (Fuera de alcance; interesa el razonamiento.)

**Caso 8**
1. ¿Por qué "hoy" se calcula en la zona horaria de México y no en UTC?
2. ¿Por qué el cálculo vive en el dominio y no en una consulta SQL?
3. Un hito aprobado y vencido, ¿cuenta dos veces en "Te deben"? ¿Por qué no?

**Caso 9**
1. ¿Por qué se guarda la descripción como texto y no se arma al consultar?
2. ¿Dónde se impide editar o borrar un evento y qué pasa si un desarrollador lo intenta desde el código?
3. ¿Por qué el orden usa una columna `Secuencia` y no solo la fecha?

**Caso 10**
1. ¿Cómo se garantiza que dos visitantes de la demo no vean los datos del otro?
2. ¿Por qué la limpieza se hace al crear una demo y no con un job programado?
3. ¿Qué falló con `NO ACTION` al borrar una demo y por qué `CASCADE` hacia el hito es seguro aquí?

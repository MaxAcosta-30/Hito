# Etapa 4 · Casos 6 a 10

**Caso 6 · Entregar y aprobar un hito:** terminado en la rama `caso-6/entregas` (2026-11-06), sin subir ni desplegar.
- Transiciones en `HitoDeProyecto`, siempre a través de `Proyecto`: exige alcance aceptado y que el hito esté en la versión vigente. Inválida → 409.
- API: `POST /proyectos/{id}/hitos/{hitoId}/entregar` con `evidenciaUrl` (solo http/https, máx. 500); `POST /publico/{token}/hitos/{hitoId}/aprobar` y `/cambios` con comentario obligatorio. Migración `AgregarEntregas`.
- Pruebas: 12 unitarias (todas las transiciones inválidas, URLs `javascript:`/`data:`/`ftp:` y las reglas del caso 5 con hitos avanzados) y 4 de integración. Total: 35 + 35.
- Web: "Marcar como entregado" con `<dialog>` nativo; el cliente ve la entrega, aprueba o pide cambios (comentario validado antes de enviar). Probado con Playwright.
- Desvío del diseño: si hay varias entregas, cada una tiene sus botones en su renglón en lugar de una sola barra fija.

**Caso 7 · Registrar un pago manual:** terminado en la rama `caso-7/pagos` (2026-11-06), sin subir ni desplegar.
- `Pago` vive en el agregado `Proyecto`: solo un hito aprobado, monto > 0 con 2 decimales y nunca más de lo pendiente. Se permiten pagos parciales.
- El hito pasa a `Pagado` al cubrirse; con todos los hitos vigentes pagados, el proyecto pasa a `Completado`. Un cancelado conserva sus pagos.
- Dos pagos simultáneos: `xmin` hace que uno responda 409 (prueba de integración con dos peticiones en paralelo).
- API: `POST /proyectos/{id}/hitos/{hitoId}/pagos`. Migración `AgregarPagos`. Pruebas: 8 unitarias y 5 de integración. Total: 43 + 40.
- Web: diálogo "Registrar pago" con el pendiente prellenado y validación al instante; totales Pagado, Por cobrar y Total. Probado con Playwright.

**Caso 8 · Panel de cobros:** terminado en la rama `caso-8/panel` (2026-11-06), sin subir ni desplegar.
- `ResumenDeCobros.Calcular` (dominio, función pura): por cobrar, aprobado sin pagar y vencido sobre lo pendiente de la versión vigente de proyectos aceptados. "Hoy" en `America/Mexico_City`.
- API: `GET /cobros`. Pruebas: 5 unitarias y 2 de integración (los totales coinciden con los datos; aislamiento). Total: 48 + 42.
- Web: cifra "Te deben", desglose y "Necesita tu atención", con esqueleto, vacío y error. Probado con Playwright.

**Pendiente de la Etapa 4:** caso 9 (historial) y caso 10 (demo).

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

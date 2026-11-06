# Etapa 4 · Casos 6 a 10

**Caso 6 · Entregar y aprobar un hito:** terminado en la rama `caso-6/entregas` (2026-11-06), sin subir ni desplegar.
- Transiciones en `HitoDeProyecto`, siempre a través de `Proyecto`: exige alcance aceptado y que el hito esté en la versión vigente. Inválida → 409.
- API: `POST /proyectos/{id}/hitos/{hitoId}/entregar` con `evidenciaUrl` (solo http/https, máx. 500); `POST /publico/{token}/hitos/{hitoId}/aprobar` y `/cambios` con comentario obligatorio. Migración `AgregarEntregas`.
- Pruebas: 12 unitarias (todas las transiciones inválidas, URLs `javascript:`/`data:`/`ftp:` y las reglas del caso 5 con hitos avanzados) y 4 de integración. Total: 35 + 35.
- Web: "Marcar como entregado" con `<dialog>` nativo; el cliente ve la entrega, aprueba o pide cambios (comentario validado antes de enviar). Probado con Playwright.
- Desvío del diseño: si hay varias entregas, cada una tiene sus botones en su renglón en lugar de una sola barra fija.

## Preguntas de entrevista pendientes

**Caso 6**
1. ¿Por qué las transiciones se hacen a través de `Proyecto` y no directamente sobre el hito?
2. ¿Qué ataque evita aceptar solo `http` y `https` en la evidencia?
3. ¿Por qué el comentario se valida en la web **y** en el servidor?

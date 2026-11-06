# 0005 · Hitos estables y términos por versión

**Estado:** propuesta · 2026-11-06 (pendiente de revisión de Max)

**Decisión:**
- `HitoDeProyecto` conserva su identidad y su estado (Pendiente → … → Pagado) durante toda la vida del proyecto. Los pagos apuntarán a él.
- Cada `VersionDeProyecto` guarda los **términos** de sus hitos (`TerminosDeHito`: título, monto, fecha, entregable). Solo se editan mientras la versión es borrador; aceptada, queda congelada.
- La versión **vigente** es la última aceptada; si nunca se aceptó, se muestra la última.
- El total se calcula en el dominio (`VersionDeProyecto.Total`) y nunca llega desde el navegador.
- Un hito que ya avanzó no puede cambiar de términos ni quitarse en una versión nueva.
- `Proyecto` usa `xmin` como token de concurrencia y cada operación toca `ActualizadoEn`, así dos cambios simultáneos responden 409.

**Por qué no hitos dentro de cada versión:** al crear la versión 2 habría que copiar estado, evidencia y pagos de cada hito y sincronizarlos si cambian mientras la versión 2 espera aprobación.
**Por qué no guardar el alcance como JSON:** los montos se suman y se validan contra pagos; en columnas `numeric(18,2)` la base los protege.
**Detalles de EF:** los Id los genera el dominio (`ValueGeneratedNever`). Términos → Hito y Pago → Hito son `CASCADE`: con `NO ACTION`, borrar un usuario de la demo fallaba porque PostgreSQL revisa esa llave antes de que la cascada por Versiones borre los términos (probado el 2026-11-06). Proyecto → Cliente queda en `NO ACTION`: un cliente no se borra, se archiva.

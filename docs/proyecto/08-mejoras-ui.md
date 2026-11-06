# Mejoras de UI/UX (después de la Etapa 6)

Se revisó una lista de buenas prácticas de UI/UX contra Hito. Max aprobó 4 mejoras (2026-11-06); cada una va en su propia rama.

**1 · Pantalla "Despertando" con identidad** (rama `ui/despertando`)
- Un sello "HITO" se estampa una vez (`transform` y `opacity`, 420 ms) y luego "respira". Con `prefers-reduced-motion` queda fijo.
- Mensajes por fase según los segundos de espera y un contador de segundos. No hay porcentaje porque no existe un progreso real que medir. Se conserva el texto que exige CLAUDE.md: "Despertando la demo, puede tardar hasta un minuto".
- Solo la región `status` anuncia las fases; el contador queda fuera para que el lector de pantalla no lea cada segundo.
- Si la API responde en menos de 400 ms, la pantalla no aparece: así no hay un destello en cada visita.
- Probado con Playwright simulando la API dormida 12 s (fases, contador y entrada) y la API despierta (la pantalla no llega a montarse).

**2 · Botón destructivo y confirmación propia** (rama `ui/destructivo`)
- Variantes nuevas: `peligro`, rojo sólido, solo para confirmar, y `peligro-contorno`, para el botón que dispara la acción, separado a la derecha de la botonera. El rojo se reserva para lo irreversible o para lo que deja al cliente sin acceso (cancelar el proyecto, revocar el enlace).
- `Confirmar` sobre el `<dialog>` nativo reemplaza los 4 `window.confirm`. Dice qué se pierde y qué se conserva, y el foco inicial va a "Volver" (`data-foco-inicial`), así un Enter accidental no destruye nada. Crear una versión y generar un enlace nuevo usan el mismo diálogo, pero con botón primario.
- Si la acción falla, el diálogo se cierra para que el error se vea en la hoja.
- Probado con Playwright: foco en "Volver", Escape no ejecuta nada, enviar → enlace nuevo → revocar → cancelar, y ningún diálogo nativo del navegador.

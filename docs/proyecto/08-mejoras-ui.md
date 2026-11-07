# Mejoras de UI/UX (después de la Etapa 6)

Se revisó una lista de buenas prácticas de UI/UX contra Hito. Max aprobó 4 mejoras (2026-11-06); cada una va en su propia rama.

**1 · Pantalla "Despertando" con identidad** (rama `ui/despertando`)
- Un sello "HITO" se estampa una vez (`transform` y `opacity`, 420 ms) y luego "respira". Con `prefers-reduced-motion` queda fijo.
- Mensajes por fase según los segundos de espera y un contador de segundos. No hay porcentaje porque no existe un progreso real que medir. Se conserva el texto que exige CLAUDE.md: "Despertando la demo, puede tardar hasta un minuto".
- Solo la región `status` anuncia las fases; el contador queda fuera para que el lector de pantalla no lea cada segundo.
- Si la API responde en menos de 400 ms, la pantalla no aparece: así no hay un destello en cada visita.
- Probado con Playwright simulando la API dormida 12 s (fases, contador y entrada) y la API despierta (la pantalla no llega a montarse).

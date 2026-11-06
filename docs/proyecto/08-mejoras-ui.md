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

**3 · Aviso de privacidad** (rama `ui/privacidad`)
- Página `/privacidad`, pública y fuera de la sesión. No espera a que despierte la API porque es texto fijo. Se enlaza desde Entrar, Registro y la vista del cliente.
- Solo describe lo que el código hace: una sola cookie (`hito_renovacion`, `httpOnly`), nada en `localStorage`, sin analítica ni correos, contraseña y token del enlace solo como hash, y datos en Oregón (Supabase y Render) con la página servida desde Vercel. La demo se borra tras 24 h.
- Es un texto informativo de un proyecto de portafolio, **no asesoría legal**. El contacto es el perfil de GitHub de Max; si quiere un correo de contacto, se cambia ahí.
- Probado con Playwright: carga con la API "dormida" y los enlaces llegan desde Entrar y Registro.

**4 · Validar al salir del campo** (rama `ui/validacion-al-salir`)
- `useValidacion` en `web/src/validacion.ts`: el error aparece al salir del campo con algo escrito, no mientras escribes ni por tabular por un campo vacío. Una vez marcado, se recalcula con cada tecla y desaparece en cuanto el valor es válido. Al enviar con errores no sale la petición y el foco va al primer campo con error.
- Se aplica en registro (nombre, correo, contraseña ≥ 10), cliente (nombre, correo, teléfono), entrega (URL http/https) y pago (monto > 0 y ≤ lo pendiente). Las reglas y mensajes copian los del servidor, que sigue validando todo.
- Si el foco pasa a un botón o un enlace, no se marca el campo. Así el mensaje no empuja "Cancelar" hacia abajo justo cuando se hace clic, que es el error clásico de validar al salir.
- Probado con Playwright: los cuatro formularios, la corrección en vivo, cero peticiones con errores, el clic en "Cancelar" tras escribir una URL inválida y el flujo e2e completo.

## Preguntas de entrevista

1. ¿Por qué la pantalla "Despertando" no muestra un porcentaje y por qué espera 400 ms antes de aparecer?
2. ¿Por qué el foco inicial del diálogo de cancelar está en "Volver" y no en la acción?
3. Si el navegador ya valida el correo, ¿por qué el servidor lo vuelve a validar?

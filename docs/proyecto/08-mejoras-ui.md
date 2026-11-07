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

## Rediseño visual "Libro mayor" (2026-11-06)

Max notó que, tras las mejoras de comportamiento, Hito se veía igual. Se propusieron 3 direcciones ([`diseno/rediseno.html`](diseno/rediseno.html)) y eligió **B · Libro mayor**. Las reglas de diseño están en `07-diseno.md`.

**Identidad** (rama `ui/identidad-libro-mayor`)
- Logo propio: un mojón (lo que literalmente significa "hito") sobre doble raya contable, en `Logo.tsx` y `favicon.svg`. Reemplaza el rayo morado de Vite.
- Paleta clara y oscura con contraste AA medido, tipografía Bricolage Grotesque, Public Sans y JetBrains Mono, sellos rectos y rellenos para lo que mueve dinero. También se actualizó la paleta de la impresión a PDF.

**Tablas y gráfica** (rama `ui/tablas-grafica`)
- Proyectos y clientes son tablas a partir de 768 px y renglones en el celular. Una de las dos versiones se oculta con `display: none`, que también la saca del árbol accesible. Las páginas con tabla se amplían a 1040 px con `:has()`.
- Gráfica "Acordado vs. cobrado": `HistorialDeCobros` (dominio, función pura) acumula por mes lo acordado que ya vencía y lo cobrado en los últimos 6 meses. Cuentan los proyectos aceptados y completados; los cancelados no. Se expone en `GET /cobros` como `historial`.
- Líneas escalonadas en SVG sin librería, con un solo eje, leyenda, etiquetas directas, un recuadro por mes y "Ver como tabla". Colores validados con el script de dataviz en claro y oscuro (banda de luminosidad, croma, daltonismo ΔE ≥ 8.7 y contraste ≥ 3:1).
- Pruebas: 2 unitarias nuevas (acumulados por mes, corte en "hoy", ignora borradores y cancelados) y una verificación de integración de que la gráfica cuadra con lo registrado. Total: 53 + 50.

**Landing pública** (rama `ui/landing`)
- `/` muestra la landing sin esperar a la API y la despierta en segundo plano mientras se lee. "Probar la demo" lleva a `/demo`, que pasa por la puerta de "Despertando" si hace falta, crea la cuenta de ejemplo una sola vez (un `ref` evita el doble efecto de StrictMode) y entra al panel.
- Elemento firma: un libro mayor en SVG donde los hitos se asientan, se sellan y aparece el saldo (solo `transform` y `opacity`; con movimiento reducido queda fijo). Debajo: "Cómo funciona" en 3 pasos, una banda oscura a todo lo ancho con la vista del cliente, la sección técnica para reclutadores y el pie con privacidad y licencia.
- Probado con Playwright: escritorio y celular, claro y oscuro, la landing carga con la API "dormida" y "Probar la demo" llega al panel con $76,000.

**Movimiento y carga** (rama `ui/movimiento`)
- **Botones:** hover con cambio de tono y sombra, solo con puntero fino (en el celular no queda "pegado"), además de la presión con `scale(.97)`.
- **Esqueletos:** con brillo que recorre el renglón, animado con `transform` y no con `background-position`.
- **"Te deben":** sube desde el valor anterior con una curva que frena al final, y el lector de pantalla solo lee el valor final. Si la animación se interrumpe, la siguiente sigue desde el último valor pintado.
- **Sellos:** se estampan solo cuando el estado cambia frente a la persona (por ejemplo, al registrar un pago); al cargar no se animan.
- **Navegación:** cada página entra suave al cambiar de ruta, y los renglones y filas entran escalonados (30 ms, tope de 150 ms) solo al montarse.
- **Panel vacío:** muestra un libro de "EJEMPLO" tenue.
- **Carga:** las pantallas internas se descargan bajo demanda (`React.lazy`) y la barra se queda mientras el contenido muestra su esqueleto. El bundle principal bajó de 110.8 a 98.6 kB comprimidos.
- Todo se desactiva con `prefers-reduced-motion`. Probado con Playwright: la cifra pinta 10 cuadros distintos y termina en $76,000, el sello PAGADO se estampa al pagar y no al cargar, y el flujo e2e sigue en verde.

**PWA instalable** (rama `ui/pwa`)
- `manifest.webmanifest` (`standalone`, inicia en `/cobros`, colores del papel), íconos de 192 y 512 px, uno adaptable (maskable) con zona segura y `apple-touch-icon` para "Agregar a inicio" en iOS. Generados con Playwright desde el SVG del logo.
- **Sin service worker:** los criterios de instalación de Chrome (web.dev, consultado el 2026-11-06) piden manifest e íconos, no service worker. Agregarlo traería caché de versiones viejas sin necesidad.
- **Límites:** es la misma web instalada, no una app nativa (la app móvil sigue fuera de alcance), y sin conexión no funciona.

**Ajustes de celular y README** (rama `ui/pwa`)
- A 390 px el logo con mojón y palabra empujaba "Salir" fuera de la pantalla; en pantallas de 420 px o menos la barra muestra solo el mojón.
- La gráfica medía 640 unidades y se encogía en el celular (textos de unos 6 px); ahora mide su contenedor con `ResizeObserver` y dibuja a escala 1:1.
- Capturas del README regeneradas con el diseño nuevo; el README menciona la gráfica, la PWA y 53 pruebas unitarias.

## Video promocional (2026-11-06)

- 40 s, 1920×1080, H.264 sin audio (sin música para no arriesgar licencias; LinkedIn reproduce en silencio). Archivo y portada en `~/Desktop/Proyectos/hito-promo/` (fuera del repositorio: es difusión, no producto).
- Se grabó la app real con Playwright dentro de un escenario con marcos de navegador y celular: landing → demo con un clic y cifra que sube → compartir el enlace → el cliente aprueba en el celular → pago con sello que se estampa → cierre con la URL. Lo reproducen `escenario.html` y `grabar.mjs` en esa carpeta.
- **Hallazgos al grabar:** el escenario debe servirse desde el mismo origen que la app (con `file://` Chrome frenaba el iframe), y los botones de un `<dialog>` modal dentro de un iframe escalado se pulsan con un clic del DOM.

**Hover que agranda y entradas tipo libro** (rama `ui/hover-y-entradas`)
- Max pidió más efectos como el libro de la landing y "que al pasar el cursor algo se haga más grande".
- **Panel de cobros:** la cifra "Te deben" sube, debajo se traza una doble raya contable y el desglose se asienta escalonado. En el detalle del proyecto, la doble raya del total se traza al cargar.
- **Hover que agranda** (solo con puntero fino y sin movimiento reducido): botones 1.03 (y 0.97 al presionar), tarjeta del saldo 1.01 con elevación, desglose 1.04, renglones 1.012, filas 1.006, sellos 1.12, el logo se levanta, y en la landing el libro se endereza, los pasos suben con su número en ámbar y la captura del celular se acerca.
- **Hallazgo:** las entradas con `animation-fill-mode: both` bloqueaban el `transform` del hover. Se cambiaron a `backwards`: el elemento queda oculto durante el retraso y libre al terminar. Medido con Playwright: tarjeta `matrix(1.01…)`, fila `1.006`, botón `1.03`, desglose `1.04`.

**Casos de ejemplo y video en el README** (rama `ui/hover-y-entradas`)
- La landing tiene la sección "Casos de ejemplo" (diseñadora, desarrollador, estudio), y el README la repite. Cada caso es un proyecto de la demo con sus sellos reales, así que se puede comprobar al abrirla.
- El README muestra la portada del video y lo enlaza al release `v1.0.0` de GitHub. El MP4 (4 MB) va como archivo del release, no dentro del repositorio.

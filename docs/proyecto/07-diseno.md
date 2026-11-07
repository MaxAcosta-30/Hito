# Etapa 2 · Diseño

**Estado:** aprobada por Max (2026-11-05). Max eligió la dirección **A · Cotización firmada**.
**Referencia visual:** [`diseno/pantallas.html`](diseno/pantallas.html), con las tres pantallas y sus estados en claro y oscuro.

## Intención

- **Freelancer:** entre una entrega y otra, quiere saber **cuánto le deben** y registrar lo que pasó. Lo usa en laptop o en el celular.
- **Cliente:** abre un enlace desde WhatsApp en el celular y debe **confiar** en un monto para aceptarlo o aprobarlo. Sin cuenta y sin ruido.
- **Sensación:** una cotización firmada sobre papel. Formal, clara y sin adornos. Cada estado se lee como un sello.

## Recorrido de pantallas

| Actor | Pantalla | Casos |
|---|---|---|
| Visitante | Inicio con "Probar la demo" | 10 |
| Freelancer | Entrar / Registrarse | 1 |
| | **Panel de cobros** (inicio) | 8 |
| | Clientes: lista y formulario | 2 |
| | Nuevo proyecto con hitos | 3 |
| | **Detalle del proyecto**: pestañas Hitos, Historial y Versiones | 3–7, 9 |
| Cliente | **Vista del cliente** (`/p/:token`): aceptar alcance, revisar entrega | 4–6 |
| | Enlace no válido (expirado o revocado; no revela datos) | 4 |

## Reglas

**Foco por pantalla.** Panel: la cifra "Te deben". Detalle: la hoja de hitos. Cliente: la única decisión pendiente, en una barra fija abajo. Cada vista tiene **una** acción primaria.

## Paleta "Grafito + Coral Rose" (2026-11-06)

Max pidió aplicar la técnica de una **base neutra tranquila + un solo acento vibrante**, con fondo **cinemático** en la landing, y no limitarse a los verdes. Entre 6 combinaciones de su paleta con el contraste medido ([`diseno/acento.html`](diseno/acento.html)) eligió **A · Grafito + Coral Rose**. Reemplaza los colores de "Libro mayor"; la tipografía, los sellos, las reglas de estados y el logo "H contable" se mantienen.

| Token | Claro | Oscuro | Uso |
|---|---|---|---|
| `--papel` / `--hoja` | Mellow White `#F1F2F6` / Snowy White `#F9F9F9` | `#15161B` / Raven Black `#1D1F26` | fondo y superficies |
| `--tinta` | Raven Black `#1D1F26` | Mellow White `#F1F2F6` | texto (15.6:1 y 14.7:1) |
| `--grafito` | `#555966` (6.2:1) | `#A6AAB6` (7.8:1) | texto secundario |
| `--acento` | Coral Rose `#FF6B82` | Coral Rose `#FF6B82` | **el único color vibrante**, solo como relleno: botón principal, sello APROBADO y logo, con texto Raven (6.0:1) |
| `--firma` | `#1D1F26` | `#FF6B82` (6.0:1) | enlaces, foco y subrayados. En claro es tinta porque el coral como texto no llega a AA (2.6:1) |
| `--sello-pagado` | relleno tinta | relleno Mellow | PAGADO: neutro, "ya está" |
| `--sello-vencido` / `--sello-cambios` | `#C21D1A` (5.7:1) / `#9A4F0E` (5.7:1) | `#FF8A7A` / Butter Yellow `#F4CD00` | rojo solo para urgencia |
| series de la gráfica | acordado `#555966` punteado (referencia), cobrado `#E8506A` | `#A6AAB6` punteado, `#E5627A` | validadas con dataviz (daltonismo ΔE ≥ 8.9; el punteado y la etiqueta directa son la segunda codificación) |

- **Landing cinemática** en ambos modos: grafito `#15161B` con la luz cálida de una lámpara y viñeta (mundo "El expediente sellado", ver `DESIGN.md`).
- Favicon, íconos de la PWA y logo: H Raven sobre cuadro coral.

## Rediseño "Libro mayor" (2026-11-06)

Max eligió la dirección **B · Libro mayor** entre tres propuestas ([`diseno/rediseno.html`](diseno/rediseno.html)). Reemplaza la paleta y la tipografía de "Cotización firmada". Las reglas de foco, estados, sellos y dinero de este documento siguen vigentes.

- **Idea:** las cifras son protagonistas, el papel es de libro contable con renglones y cada hito se "asienta".
- **Logo:** "H contable": una H cuyo travesaño es la doble raya de los totales, sobre un cuadro verde (`web/src/componentes/Logo.tsx`, `favicon.svg` e íconos de la PWA). Max la eligió el 2026-11-06 entre 4 propuestas ([`diseno/logos.html`](diseno/logos.html)) para reemplazar el mojón, que se leía como lápida y no se distinguía a 16 px.
- Se cambiaron los valores de los tokens existentes, no sus nombres, así que las pantallas no tuvieron que reescribirse.

**Tokens** (oscuro = "libro de noche", diseñado aparte):

| Token | Claro | Oscuro | Uso |
|---|---|---|---|
| `--papel` | `#EDF1EA` | `#0E1813` | fondo, con pauta de renglones cada 32 px (`--pauta`) |
| `--hoja` | `#F7F9F4` | `#14221B` | superficies |
| `--campo` | `#E6EBE2` | `#0B140F` | interior de inputs |
| `--tinta` | `#10231B` | `#E4EBE2` | texto principal |
| `--grafito` | `#4F6358` | `#9DB0A4` | texto secundario |
| `--firma` | `#0F6B4A` | `#5FC79B` | acento único: acción primaria, foco, enlaces y logo |
| `--ambar` | `#C77D12` | `#E0A341` | solo como **relleno** del sello APROBADO, con texto `--sobre-ambar` (en claro, como texto no llega a AA: 3.1:1) |
| `--sello-pagado` | `#0F6B4A` | `#5FC79B` | PAGADO, relleno |
| `--sello-vencido` | `#B5442C` | `#F08A73` | VENCIDO, errores y acciones destructivas |
| `--sello-cambios` | `#9A4F0E` | `#E0A341` | CAMBIOS |

Contraste medido el 2026-11-06: todos los textos ≥ 4.5:1 en ambos modos (grafito sobre papel 5.6, botón verde 6.2, sello ámbar 5.0).

**Tipografía** (autoalojada con `@fontsource`):
- **Bricolage Grotesque 800:** títulos, con tracking negativo (−0.03em) y el logo (−0.04em).
- **Public Sans 400/600:** cuerpo y formularios.
- **JetBrains Mono 500/700** con `tabular-nums`: todos los montos y los sellos.

**Forma:** radios rectos (control 2, hoja 3, diálogo 6). Los sellos son rectos y sin rotación; los que mueven dinero (APROBADO y PAGADO) van rellenos.

## Reglas previas ("Cotización firmada", 2026-11-05)

**Tokens** (nombres del mundo del producto; oscuro = "escritorio de noche"):

| Token | Claro | Oscuro | Uso |
|---|---|---|---|
| `--papel` | `#F5F0E6` | `#151824` | fondo de la página |
| `--hoja` | `#FBF8F1` | `#1C2030` | superficies (tarjetas, diálogos) |
| `--campo` | `#F1EBDF` | `#12151F` | interior de inputs (más oscuro: "aquí se escribe") |
| `--tinta` | `#1D2B4F` | `#ECE6D8` | texto principal |
| `--grafito` | `#6B6558` | `#A9A293` | texto secundario y metadatos |
| `--renglon` / `--renglon-fuerte` | tinta al 12 % / 24 % | papel al 10 % / 22 % | bordes |
| `--firma` | `#2347A6` | `#93ABF2` | **único acento**: acción primaria, foco, enlaces, sello APROBADO |
| `--sello-pagado` | `#2F6B3F` | `#86C595` | PAGADO |
| `--sello-vencido` | `#B23A2E` | `#EE9184` | VENCIDO y errores |
| `--sello-cambios` | `#8F5400` | `#E2B060` | CAMBIOS |

Todos los textos superan 4.5:1 (WCAG AA) en ambos modos; se midió el 5 de noviembre.

**Tipografía** (autoalojada con `@fontsource`, sin pedir nada a Google en producción):
- **Newsreader 600:** títulos (h1 31 · h2 25 · h3 20).
- **Source Sans 3:** cuerpo de 16 px (en móvil nunca menos de 16 en inputs), metadatos de 14 y rótulos de 12/600 en mayúsculas con espaciado de 0.1em.
- **IBM Plex Mono** con `tabular-nums`: **todo monto**. Cifra protagonista de 39/600.
- Escala de 1.25. La jerarquía se construye con tamaño, peso y color juntos.

**Espaciado y forma:**
- Base de 4 px; padding de 16 en móvil y 24 en escritorio; 16 entre hojas.
- Profundidad **solo con bordes** (renglones de documento) y una sombra mínima en claro.
- Radios: control 4 · hoja 8 · diálogo 12.
- Ancho de contenido máximo de 720 px y centrado. Sin barra lateral: arriba la marca y 3 enlaces (Cobros, Proyectos, Clientes).

**Sellos** (solo para el estado del **hito**; siempre con texto, nunca solo color):

| Estado | Sello |
|---|---|
| Pendiente | `PENDIENTE`, grafito, borde punteado |
| Entregado | `ENTREGADO`, grafito, borde discontinuo |
| CambiosSolicitados | `CAMBIOS`, sello-cambios |
| Aprobado | `APROBADO`, firma |
| Pagado | `PAGADO`, sello-pagado |
| Vencido (fecha pasada y sin pagar) | `VENCIDO`, sello-vencido; se suma al estado |

El estado del **proyecto** va como texto en la cabecera ("Versión 2 · aceptada el 1 nov"), no con sello.

**Dinero y fechas:**
- Montos con `Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' })`, siempre con 2 decimales.
- Fechas cortas en es-MX ("4 nov").
- Los totales llevan doble raya contable (`border-bottom: 3px double`).

**Estados obligatorios:**
- **Carga:** renglones vacíos (esqueleto), sin spinners.
- **Vacío:** explica qué sigue y ofrece una acción.
- **Error:** dice qué pasó y ofrece "Reintentar"; los errores de campo van bajo el campo con `aria-describedby`.
- **Éxito:** el sello cambia en su renglón y aparece un toast breve.
- **Despertando:** "Despertando la demo, puede tardar hasta un minuto".

**Interacción:**
- Áreas táctiles de 44 px como mínimo.
- Foco visible con un anillo `--firma` de 2 px.
- Botón al presionar: `scale(.97)` en 120 ms.
- Transiciones de menos de 250 ms, solo de `transform` y `opacity`.
- Se respeta `prefers-reduced-motion`.
- El comentario para "Pedir cambios" es obligatorio y se valida antes de enviar.

## Siguiente paso

Aprobar el diseño y pasar a la Etapa 3, caso 1 (registro e inicio de sesión), en una sesión nueva.

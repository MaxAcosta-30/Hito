# Etapa 2 · Diseño

**Estado:** pendiente de aprobación (2026-11-05). Max eligió la dirección **A · Cotización firmada**.
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

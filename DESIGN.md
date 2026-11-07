---
name: Hito
description: Cada entrega aprobada, cada cobro claro.
colors:
  coral-rose: "#ff6b82"
  coral-pestana: "#f0627a"
  coral-tinta-sello: "#d23a55"
  raven-tinta: "#1d1f26"
  grafito: "#555966"
  grafito-claro: "#a6aab6"
  papel-mellow: "#f1f2f6"
  hoja-nieve: "#f9f9f9"
  campo: "#e7e8ee"
  escritorio-noche: "#15161b"
  tinta-sobre-coral: "#2b1a1f"
  renglon: "rgba(29, 31, 38, 0.12)"
  renglon-fuerte: "rgba(29, 31, 38, 0.28)"
  sello-vencido: "#c21d1a"
  sello-cambios: "#9a4f0e"
  serie-cobrado: "#e8506a"
typography:
  display:
    fontFamily: "'Bricolage Grotesque', system-ui, sans-serif"
    fontSize: "clamp(44px, 6.6vw, 94px)"
    fontWeight: 800
    lineHeight: 0.93
    letterSpacing: "-0.04em"
  headline:
    fontFamily: "'Bricolage Grotesque', system-ui, sans-serif"
    fontSize: "clamp(32px, 4.4vw, 56px)"
    fontWeight: 800
    lineHeight: 1
    letterSpacing: "-0.035em"
  headline-app:
    fontFamily: "'Bricolage Grotesque', system-ui, sans-serif"
    fontSize: "31px"
    fontWeight: 800
    letterSpacing: "-0.03em"
  title:
    fontFamily: "'Bricolage Grotesque', system-ui, sans-serif"
    fontSize: "25px"
    fontWeight: 800
    letterSpacing: "-0.03em"
  title-sm:
    fontFamily: "'Bricolage Grotesque', system-ui, sans-serif"
    fontSize: "20px"
    fontWeight: 800
    letterSpacing: "-0.03em"
  body:
    fontFamily: "'Public Sans', system-ui, sans-serif"
    fontSize: "16px"
    fontWeight: 400
    lineHeight: 1.5
  body-lead:
    fontFamily: "'Public Sans', system-ui, sans-serif"
    fontSize: "clamp(17px, 1.5vw, 20px)"
    fontWeight: 400
    lineHeight: 1.5
  label:
    fontFamily: "'Public Sans', system-ui, sans-serif"
    fontSize: "12px"
    fontWeight: 600
    lineHeight: 1
    letterSpacing: "0.1em"
  button:
    fontFamily: "'Public Sans', system-ui, sans-serif"
    fontSize: "15px"
    fontWeight: 600
  cifra:
    fontFamily: "'JetBrains Mono', ui-monospace, monospace"
    fontSize: "39px"
    fontWeight: 600
    lineHeight: 1.1
    letterSpacing: "-0.02em"
    fontFeature: "tnum"
  importe:
    fontFamily: "'JetBrains Mono', ui-monospace, monospace"
    fontSize: "15px"
    fontWeight: 500
    fontFeature: "tnum"
  folio:
    fontFamily: "'JetBrains Mono', ui-monospace, monospace"
    fontSize: "12px"
    fontWeight: 500
    letterSpacing: "0.02em"
    fontFeature: "tnum"
  sello:
    fontFamily: "'JetBrains Mono', ui-monospace, monospace"
    fontSize: "11px"
    fontWeight: 700
    lineHeight: 1
    letterSpacing: "0.1em"
  sello-golpe:
    fontFamily: "'Bricolage Grotesque', system-ui, sans-serif"
    fontSize: "26px"
    fontWeight: 800
    lineHeight: 1
    letterSpacing: "0.08em"
rounded:
  sello: "2px"
  control: "4px"
  papel: "4px"
  hoja: "6px"
  pestana-app: "8px 8px 0 0"
  pestana-carpeta: "10px 10px 0 0"
  dialogo: "10px"
  carpeta: "0 14px 14px 14px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
  pauta: "32px"
  seccion: "clamp(64px, 9vw, 128px)"
components:
  button-primary:
    backgroundColor: "{colors.coral-rose}"
    textColor: "{colors.raven-tinta}"
    typography: "{typography.button}"
    rounded: "{rounded.control}"
    padding: "0 16px"
    height: "44px"
  button-tinta:
    backgroundColor: "{colors.raven-tinta}"
    textColor: "{colors.hoja-nieve}"
    typography: "{typography.button}"
    rounded: "{rounded.control}"
    padding: "0 22px"
    height: "52px"
  button-secondary:
    backgroundColor: "transparent"
    textColor: "{colors.raven-tinta}"
    typography: "{typography.button}"
    rounded: "{rounded.control}"
    padding: "0 16px"
    height: "44px"
  input:
    backgroundColor: "{colors.campo}"
    textColor: "{colors.raven-tinta}"
    typography: "{typography.body}"
    rounded: "{rounded.control}"
    padding: "10px 12px"
    height: "44px"
  hoja:
    backgroundColor: "{colors.hoja-nieve}"
    textColor: "{colors.raven-tinta}"
    rounded: "{rounded.hoja}"
    padding: "24px"
  sello-pendiente:
    backgroundColor: "transparent"
    textColor: "{colors.grafito}"
    typography: "{typography.sello}"
    rounded: "{rounded.sello}"
    padding: "5px 7px"
  sello-entregado:
    backgroundColor: "transparent"
    textColor: "{colors.grafito}"
    typography: "{typography.sello}"
    rounded: "{rounded.sello}"
    padding: "5px 7px"
  sello-cambios:
    backgroundColor: "transparent"
    textColor: "{colors.sello-cambios}"
    typography: "{typography.sello}"
    rounded: "{rounded.sello}"
    padding: "5px 7px"
  sello-aprobado:
    backgroundColor: "{colors.coral-rose}"
    textColor: "{colors.raven-tinta}"
    typography: "{typography.sello}"
    rounded: "{rounded.sello}"
    padding: "5px 7px"
  sello-pagado:
    backgroundColor: "{colors.raven-tinta}"
    textColor: "{colors.hoja-nieve}"
    typography: "{typography.sello}"
    rounded: "{rounded.sello}"
    padding: "5px 7px"
  sello-vencido:
    backgroundColor: "transparent"
    textColor: "{colors.sello-vencido}"
    typography: "{typography.sello}"
    rounded: "{rounded.sello}"
    padding: "5px 7px"
  pestana-carpeta:
    backgroundColor: "{colors.coral-pestana}"
    textColor: "{colors.raven-tinta}"
    typography: "{typography.button}"
    rounded: "{rounded.pestana-carpeta}"
    padding: "10px 18px 12px"
  pestana-carpeta-activa:
    backgroundColor: "{colors.coral-rose}"
    textColor: "{colors.raven-tinta}"
  toast:
    backgroundColor: "{colors.raven-tinta}"
    textColor: "{colors.hoja-nieve}"
    rounded: "{rounded.hoja}"
    padding: "12px 16px"
---

# Design System: Hito

## Overview

**Creative North Star: "El expediente sellado"**

Cada proyecto es un expediente: lo acordado se sella, el folio corre y nada cambia sin una versión nueva. El sistema se construye con cuatro materiales físicos y nada más: un escritorio grafito con luz de lámpara, una carpeta coral con pestañas, hojas blanco nieve con renglones y sellos de goma con tinta real. Los números se escriben en mono, como folios y asientos de un libro contable.

Hay dos modos. La landing **persuade**: escritorio oscuro siempre (aunque el sistema esté en claro), carpeta coral a sangre, hojas giradas que caen sobre el escritorio, sellos que se estampan con golpe y una cifra que baja en vivo. La app **opera con contención**: papel claro con pauta de 32 px (y un modo oscuro grafito diseñado aparte), hojas planas, renglones, una cifra protagonista por pantalla y sellos chicos que solo se estampan cuando el estado cambia frente a la persona. La marca vive en detalles precisos: pestañas de carpeta en la navegación, el sello LISTO del aviso, el logo H contable.

Rechazos confirmados: el héroe partido de SaaS (titular + captura + fila de tarjetas), la plantilla genérica, el folclor de cartón y el aspecto de juguete.

**Key Characteristics:**
- Materiales, no tarjetas: escritorio, carpeta, hoja, sello, clip.
- Un solo acento vibrante (Coral Rose) que funciona como material (la carpeta) o como relleno (botón principal, sello APROBADO, logo), nunca como texto en claro.
- El estado se lee como sello con texto, nunca solo por color.
- Mono solo para cifras, folios y la letra de los sellos.
- El movimiento es un golpe de sello: cae, aprieta la hoja y se queda.

## Colors

Base neutra grafito tranquila y un solo acento vibrante: Grafito + Coral Rose.

### Primary
- **Coral Rose** (`coral-rose`): el único color vibrante. En la landing es el material de la carpeta; en la app, solo relleno: botón principal, sello APROBADO, sello LISTO del aviso, subrayado de la pestaña activa, símbolo del logo, selección de texto y cursor de escritura. Siempre con texto Raven encima.
- **Coral pestaña** (`coral-pestana`): un tono más hundido para las pestañas inactivas de la carpeta y las pestañas de las carpetas de ejemplo; la activa toma Coral Rose y se une al cuerpo.
- **Tinta coral de sello** (`coral-tinta-sello`): solo la tinta del sello grande APROBADO que cae sobre la hoja, con mezcla multiplicar para que se lea como tinta absorbida por el papel.

### Neutral
- **Raven** (`raven-tinta`): texto, botón tinta de la landing, relleno del sello PAGADO y del aviso. En claro también es la `firma` (enlaces, foco, subrayados).
- **Grafito** (`grafito`): texto secundario, sellos PENDIENTE y ENTREGADO, línea de referencia "Acordado" en la gráfica (punteada 6/4).
- **Grafito claro** (`grafito-claro`): texto secundario sobre el escritorio de la landing y en modo oscuro.
- **Papel Mellow** (`papel-mellow`): fondo de la app en claro, con pauta de renglón cada 32 px; texto principal sobre el escritorio.
- **Hoja nieve** (`hoja-nieve`): toda hoja, en claro y en la landing.
- **Campo** (`campo`): fondo de inputs.
- **Escritorio noche** (`escritorio-noche`): el escritorio de la landing y el fondo de la app en oscuro (allí la hoja es Raven y el campo `#121318`).
- **Tinta sobre coral** (`tinta-sobre-coral`): texto secundario sobre la carpeta coral (bajada y nota).
- **Renglón / renglón fuerte**: divisores de renglón y bordes de controles; en oscuro pasan a Mellow al 10 % y 24 %.

### Estados y datos
- **Rojo vencido** (`sello-vencido`): solo urgencia: sello VENCIDO, errores de campo, cifra de vencido. En oscuro `#ff8a7a`.
- **Ámbar cambios** (`sello-cambios`): sello CAMBIOS y el fondo al 10 % del comentario de cambios. En oscuro Butter Yellow `#f4cd00`.
- **Serie cobrado** (`serie-cobrado`): la línea "Cobrado" de la gráfica. En oscuro `#e5627a`.

### Named Rules
**The Coral Is Material Rule.** El coral es relleno o material, nunca texto ni borde fino en modo claro (2.6:1 sobre papel). En oscuro sí puede ser texto y foco (`firma`, 6.0:1).

**The Paper Stays Paper Rule.** Toda hoja que vive sobre el escritorio de la landing es clara aunque el sistema esté en oscuro: la hoja redefine sus propios tokens a los valores claros.

**The One Reference Rule.** En gráficas, lo esperado (Acordado) es grafito punteado; solo lo real (Cobrado) lleva color.

## Typography

**Display Font:** Bricolage Grotesque 800 (con system-ui)
**Body Font:** Public Sans 400 y 600 (con system-ui)
**Label/Mono Font:** JetBrains Mono 500 y 700 (con ui-monospace)

Todas autoalojadas con `@fontsource` (CSP estricta, fuentes solo propias).

**Character:** una grotesca de peso negro y tracking cerrado que se siente estampada, sobre una sans pública y sobria que no estorba; la mono es la letra del libro contable.

### Hierarchy
- **Display** (800, clamp 44–94 px, 0.93): solo el titular de la carpeta, máx. 11ch; en celular 40 px.
- **Headline** (800, clamp 32–56 px, 1): títulos de sección sobre el escritorio, máx. 18ch. En la app, el título de página es 31 px.
- **Title** (800, 25 px; 20 px en diálogos y vacíos; 24 px en la hoja del expediente).
- **Body** (400, 16 px, 1.5): texto corrido; la bajada de la landing sube a 17–20 px, máx. 40–52ch.
- **Label** (600, 12 px, tracking 0.1em, mayúsculas): rótulo que nombra una cifra o un bloque de datos ("Te deben", "Vencido").
- **Cifra** (mono 600, 39 px, tabular): la cifra protagonista del panel; el total de la hoja de la landing es mono 700, 26–34 px.
- **Importe / folio** (mono 500, 12–15 px, tabular): montos en renglones y el folio corrido.
- **Sello** (mono 700, 11 px, tracking 0.1em): letra de los sellos chicos; 20–22 px con borde 3 px en los grandes.

### Named Rules
**The Ledger Mono Rule.** Mono solo para cifras, folios, encabezados de tabla y la letra de los sellos; jamás para prosa ni títulos.

**The Label Names Data Rule.** El rótulo en mayúsculas nombra una cifra o un bloque de datos; nunca va encima de un titular como antetítulo.

## Layout

App: una columna de lectura de 720 px máx. (420 px en entrar/registro), padding 16 px que sube a 24 px desde 640 px; ritmo 4/8/12/16/24 y pauta de renglón de 32 px en el fondo. En celular la información va en renglones; desde 768 px, en tablas. El diálogo sube desde abajo en celular y se centra (480 px) desde 640 px. Desde 420 px hacia abajo la barra muestra solo el símbolo del logo.

Landing: contenedor de 1240 px con márgenes clamp(16px, 4vw, 40px) y secciones separadas por clamp(64px, 9vw, 128px). La carpeta ocupa el ancho bajo sus pestañas; desde 960 px se parte en etiqueta (1.1fr) y hoja (0.9fr), y la hoja sobresale 72 px por debajo de la carpeta hacia el escritorio. Las carpetas de ejemplo se apilan con -14 px de traslape y desde 900 px se abren en abanico de tres columnas con -48 px de traslape y desniveles de 14 y 44 px. El celular de la vista del cliente se apoya sobre una hoja (columnas 7–12 de 12 sobre la hoja 1–9). En 520 px o menos se comprime la primera pantalla para que el botón Aprobar entrega quede visible.

**The First Viewport Rule.** En celular, el momento memorable (aprobar y ver bajar lo que te deben) cabe en la primera pantalla.

## Elevation & Depth

Híbrido según el modo. La app es plana: la hoja lleva una sombra mínima en claro y ninguna en oscuro; la profundidad sale de bordes de renglón y del fondo pautado. La landing es física: escritorio con viñeta y un charco de luz cálida de lámpara arriba a la izquierda; la carpeta, las hojas y el celular proyectan sombras largas y suaves sobre ese escritorio, y el papel lleva fibra de ruido y renglones tenues.

### Shadow Vocabulary
- **Hoja de app** (`0 1px 2px rgba(29,31,38,.06)`): solo en claro.
- **Papel sobre escritorio** (`inset 0 1px 0 rgba(255,255,255,.6), 0 18px 40px rgba(0,0,0,.35)`): toda hoja de la landing.
- **Carpeta** (`0 34px 70px rgba(0,0,0,.5)`): el cuerpo de la carpeta.
- **Celular** (`-18px 30px 60px rgba(0,0,0,.55)`): el teléfono apoyado.
- **Respuesta al puntero** (`0 6px 16px rgba(0,0,0,.28)` en el botón tinta; `0 10px 28px` tinta 12 % en la hoja de saldo): solo en hover con puntero fino.

### Named Rules
**The Flat Desk Rule.** En la app las superficies son planas en reposo; la sombra solo aparece como respuesta al puntero. Las sombras largas son exclusivas de los objetos sobre el escritorio de la landing.

## Shapes

Esquinas casi rectas, de papelería: sello 2 px, control 4 px, papel de la landing 4 px, hoja de app 6 px, diálogo 10 px. Las pestañas tienen solo las esquinas de arriba redondeadas (8 px en la app, 10 px en la carpeta) y la carpeta abre la esquina superior izquierda donde nace la pestaña (0 14 14 14). Los objetos sobre el escritorio están apenas girados (hojas entre -0.6° y 1.2°, carpetas de -1.2° a 1°, celular 5°); los sellos chicos van a -1.2° y el segundo de un par a 0.8°; los sellos grandes, entre -6° y 2°. El total se cierra con doble raya (borde doble de 5 px), el mismo gesto del travesaño del logo.

**The Barely Turned Rule.** Nada se gira más de 6°; lo girado es papel o tinta, nunca un control.

## Components

### Buttons
- **Shape:** control de 4 px, mínimo 44 px de alto (52 px en la variante grande).
- **Primario:** relleno Coral Rose, texto Raven; una sola acción primaria por vista. Hover: coral mezclado al 88 % con tinta.
- **Tinta:** relleno Raven, texto nieve; solo sobre la carpeta coral, donde el coral ya es el material. Hover `#2c2f39` con sombra suave.
- **Secundario / contorno:** transparente con borde renglón fuerte (o `currentColor` sobre el escritorio); hover tiñe al 8 %.
- **Texto:** color firma, subrayado en hover.
- **Press:** escala 0.97 en 120 ms; hover escala 1.03 solo con puntero fino y sin movimiento reducido.

### Inputs / Fields
- **Style:** fondo campo, borde renglón fuerte, 4 px, 44 px, texto 16 px; montos en mono.
- **Focus:** contorno de 2 px en color firma, separado 2 px.
- **Error:** borde y mensaje en rojo vencido, mensaje 14 px 600.

### Navigation
- **App:** pestañas de carpeta: la activa toma el color del papel, se une a la hoja de abajo y lleva una raya coral de 2 px; las demás en grafito 600 15 px.
- **Landing:** las pestañas de la carpeta son la navegación de secciones; la activa es Coral Rose, las demás coral pestaña y suben 3 px en hover. En celular se desplazan con el borde desvanecido.
- **Pestañas internas** (Hitos, Historial, Versiones): subrayado inset de 2 px en firma.

### Cards / Containers (hojas)
- **App:** hoja nieve (Raven en oscuro), borde renglón, 6 px, padding 16 → 24 px. Dentro, renglones con borde superior y monto mono alineado a la derecha.
- **Landing:** papel con fibra y renglones, 4 px, sombra de escritorio, apenas girado; el expediente lleva clip de metal.

### Sellos (signature)
La gramática del estado del hito, siempre con texto:
- **Punteado grafito** = PENDIENTE.
- **Discontinuo grafito** = ENTREGADO.
- **Contorno ámbar** = CAMBIOS.
- **Relleno** = el dinero se movió o ya se debe: APROBADO en coral, PAGADO en Raven (Mellow en oscuro).
- **Contorno rojo** = VENCIDO, junto al sello de estado, nunca en su lugar.

Los sellos chicos usan el filtro `#tinta-fina` (borde irregular, sin manchas, legibles siempre); los grandes usan `#tinta` (borde y cobertura con manchas). Ambos filtros SVG se montan una sola vez. El sello grande que cae sobre el renglón usa la letra display 800 a 26 px, borde 3 px, mezcla multiplicar y opacidad final 0.88; es decorativo (`aria-hidden`) y el estado accesible es el sello chico de la columna.

### Toast
Barra Raven abajo al centro (480 px máx.), 3.5 s, región `role="status"` siempre montada; abre con un sello LISTO coral.

### Motion
- **Respuesta de UI:** `cubic-bezier(0.23, 1, 0.32, 1)`: press 120 ms, color 150 ms, entrada 200 ms (subir 8 px), pestaña y sello en hover 160 ms, carpeta de ejemplo 260 ms.
- **Golpe de sello:** `cubic-bezier(0.16, 1, 0.3, 1)`: sello que cambia 360 ms desde escala 1.8; sello grande 420 ms (1.8 → 0.97 → 1); "asentado." del titular 520 ms con 350 ms de espera; la hoja se aprieta a 0.988 en 320 ms tras 180 ms; el folio corre 380 ms.
- **Scroll:** en "Así se arma" cada sello cae sobre su renglón con `animation-timeline: view()` (entry 40 % a cover 35 %), solo donde el navegador lo soporta.
- **Movimiento reducido:** todas las animaciones de sello, golpe, folio, titular, toast y diálogo se apagan; los sellos quedan estampados en su lugar. Hover solo con `(hover: hover) and (pointer: fine)`.

## Do's and Don'ts

### Do:
- **Do** escribir cada estado como sello con texto, siguiendo la gramática: relleno = dinero, discontinuo = entregado, punteado = pendiente, contorno rojo = vencido.
- **Do** usar Coral Rose solo como relleno o material, con texto Raven encima.
- **Do** poner cifras, folios e importes en JetBrains Mono con números tabulares.
- **Do** dar a cada vista una sola cifra protagonista y una sola acción primaria.
- **Do** ajustar el color del foco al material: claro sobre el escritorio, Raven sobre la carpeta y el papel.
- **Do** estampar solo cuando el estado cambia frente a la persona; al cargar, los sellos ya están ahí.
- **Do** mantener 44 px de área táctil mínima y WCAG AA en claro y oscuro.

### Don't:
- **Don't** usar el héroe partido de SaaS (titular + captura + fila de tarjetas).
- **Don't** usar coral como texto o borde fino en modo claro.
- **Don't** dar color a una serie de referencia: lo acordado es grafito punteado.
- **Don't** poner la mono en prosa, titulares o nombres.
- **Don't** usar la tinta con manchas (`#tinta`) en sellos chicos: deben leerse siempre.
- **Don't** girar controles ni pasar de 6° en papel y tinta.
- **Don't** usar glifos Unicode como íconos: los íconos son SVG de un trazo de 1.8.
- **Don't** colocar un rótulo en mayúsculas encima de un titular como antetítulo.

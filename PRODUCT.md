# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Freelancers creativos y técnicos en México (diseño, desarrollo, fotografía, estudios pequeños) que cobran por proyecto y hoy persiguen acuerdos y pagos por WhatsApp. Su cliente, casi siempre una pyme, no tiene cuenta: abre un enlace desde el celular para aceptar el alcance y aprobar entregas. (Confirmado por Max, 2026-11-06.) Público secundario real hoy: reclutadores que evalúan el proyecto como portafolio de Max.

## Product Purpose

Que el freelancer acuerde hitos con su cliente, registre cada entrega y aprobación, y sepa en todo momento cuánto le deben. Éxito: el cliente acepta y aprueba sin fricción y el freelancer cobra sin discutir lo que se acordó.

## Positioning

El alcance aceptado queda congelado y cada estado queda "asentado" como en un libro contable: aprobación con un enlace, sin cuenta, y un panel que separa lo por cobrar, lo aprobado sin pagar y lo vencido. No es facturación ni cobro con tarjeta.

## Operating Context

- El freelancer trabaja en laptop o celular entre entregas; el cliente decide en el celular, a veces con luz de día.
- El enlace se comparte por WhatsApp; vence y se puede revocar.
- Montos en MXN; fechas en es-MX; zona horaria de México.

## Capabilities and Constraints

- 10 casos: cuentas, clientes, proyectos con hitos, envío por enlace, aceptación con versiones, entregas y aprobaciones, pagos manuales, panel de cobros, historial inmutable y demo con un clic. Además: búsqueda y filtros, PDF, PWA instalable.
- Stack: React 19 + Vite + TanStack Query (web), .NET 10 (API), PostgreSQL. Presupuesto $0: Vercel Hobby, Render Free (arranque en frío), Supabase Free.
- Fuera de alcance: cobros con tarjeta, CFDI, correos, tiempo real, equipos, app nativa.
- CSP estricta (`script-src 'self'`, fuentes solo propias).

## Brand Commitments

- Nombre: **Hito**. Frase: "Cada entrega aprobada, cada cobro claro."
- Logo "H contable": una H cuyo travesaño es la doble raya de los totales.
- Voz: **cómplice y directa**, de tú, con humor ligero mexicano, sin perder seriedad con el dinero (confirmado por Max, 2026-11-06).
- Max pidió que la UI tenga personalidad, alma y vida, y que sea de primer nivel.

## Evidence on Hand

- Demo con datos de ejemplo (3 clientes, 3 proyectos) y video de 39 s en el release v1.0.0.
- No hay testimonios, clientes reales, métricas de uso ni precios: no se inventan.

## Product Principles

1. Lo acordado no se mueve: cada cambio es una versión nueva que el cliente acepta.
2. El dinero se lee de un vistazo: una cifra protagonista y solo lo que pide acción.
3. El cliente decide sin fricción: un enlace, sin cuenta, desde el celular.
4. Todo queda asentado: quién, qué y cuándo, sin editar ni borrar.

## Accessibility & Inclusion

WCAG AA en claro y oscuro; estado nunca solo por color (sellos con texto); `prefers-reduced-motion` respetado; uso táctil (44 px).

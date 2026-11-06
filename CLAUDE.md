# Hito

Sistema para que un freelancer acuerde hitos con su cliente, registre cada entrega y aprobación, y sepa qué le deben.

**Frase del proyecto:** "Cada entrega aprobada, cada cobro claro."

## Propósito

Este es un proyecto de portafolio de Max Acosta para aplicar a puestos full stack .NET + React. Debe servir como evidencia de que sabe construir, publicar y defender un sistema real. Su presupuesto es de $0 al mes y tiene entre 130 y 175 horas para este proyecto.

## Cómo debes trabajar

1. Trabaja una etapa a la vez. Al terminar cada una, muestra el resultado y **espera la aprobación de Max**.
2. Explica cada decisión importante en una línea: por qué se toma y por qué no otra opción.
3. Antes de programar cada caso, explica el enfoque. Después, hazle a Max 3 preguntas de entrevista sobre lo que se construyó y corrige sus respuestas.
4. Haz el cambio mínimo. No agregues abstracciones, servicios ni librerías "por si acaso".
5. Usa una rama por caso, commits pequeños y `main` siempre funcional.
6. Registra las decisiones grandes en `docs/decisiones/` como ADR cortos y el avance por etapa en `docs/proyecto/`.
7. Nunca guardes secretos en el repositorio. No inventes resultados: si algo no se probó, dilo.
8. Antes de crear recursos en la nube, verifica precios y límites vigentes. Si un servicio pide tarjeta o puede generar un cobro, detente y pregunta.
9. Si el proyecto va atrasado, recorta primero los casos "Debería" y avísale a Max.

## Punto de partida

- **Repositorio nuevo:** crea `hito` y no reutilices el historial de FreelanceGuard-ERP.
- **FreelanceGuard-ERP:** usa solo la idea de proteger el trabajo del freelancer. Tiene 2 endpoints y su función central es un bloqueo remoto por licencia. **No copies** ese bloqueo, Redis, la URL del Gist ni la contraseña de `docker-compose.yml`.
- **SafeLedger-API:** úsalo como referencia para la estructura en capas, EF Core y pruebas. Revisa su licencia y adapta solo lo que se justifique.

Clona ambos fuera de la carpeta de `hito`:

```bash
git clone https://github.com/MaxAcosta-30/FreelanceGuard-ERP.git ../referencias/FreelanceGuard-ERP
git clone https://github.com/MaxAcosta-30/SafeLedger-API.git ../referencias/SafeLedger-API
```

## Alcance

### Actores

- **Freelancer:** tiene cuenta y administra sus clientes, proyectos y cobros.
- **Cliente:** no tiene cuenta. Entra con un enlace privado.
- **Visitante:** prueba la demo sin registrarse.

### Debe

| # | Caso | Criterio de aceptación |
|---|---|---|
| 1 | Registrarse e iniciar sesión | Un usuario no puede leer ni modificar datos de otro; hay una prueba automatizada que lo comprueba |
| 2 | Administrar clientes | Crear, editar, listar y archivar, con validación de datos |
| 3 | Crear un proyecto con hitos | Cada hito tiene título, monto en MXN, fecha y descripción del entregable; el total se calcula en el servidor |
| 4 | Enviar el proyecto al cliente | El enlace es privado, expira y se puede revocar; en la base solo se guarda el hash del token |
| 5 | Aceptar el alcance | Al aceptar, los hitos y montos quedan congelados; un cambio posterior crea una nueva versión que debe aceptarse |
| 6 | Entregar y aprobar un hito | El freelancer agrega una URL de evidencia; el cliente aprueba o pide cambios con un comentario obligatorio |
| 7 | Registrar un pago manual | Solo se puede pagar un hito aprobado y no se puede pagar de más |
| 8 | Ver el panel de cobros | Muestra por cobrar, aprobado sin pagar y vencido; los totales coinciden con los datos |
| 9 | Consultar el historial | Cada acción guarda quién, qué y cuándo; la API no permite editar ni borrar eventos |
| 10 | Probar la demo | Con un clic crea una cuenta temporal con datos de ejemplo, aislada de otras sesiones |

### Debería

- Exportar a PDF el resumen de un proyecto.
- Filtrar y buscar proyectos.

### Fuera de alcance

- Cobros reales con tarjeta, facturación o CFDI.
- Envío de correos.
- Tiempo real.
- Equipos de varios usuarios.
- App móvil.

### Reglas de estado

- **Proyecto:** `Borrador → Enviado → Aceptado → Completado`. También puede pasar a `Cancelado`, y los proyectos con pagos registrados conservan su historial.
- **Hito:** `Pendiente → Entregado → Aprobado → Pagado`. Desde `Entregado` puede pasar a `CambiosSolicitados`, y de ahí volver a `Entregado`.
- Estas reglas viven en el dominio y tienen pruebas unitarias. Una transición inválida responde 409.

## Arquitectura

```text
Navegador
  └─ React en Vercel Hobby ── /api/* (rewrite) ──> API .NET en Render Free ──> PostgreSQL en Supabase (session pooler)

GitHub Actions: pruebas → prueba en Kubernetes (kind) → migraciones (bundle de EF Core) contra Supabase → deploy hook de Render
GitHub Actions programado: llama a /health/ready dos veces por semana
```

### Decisiones

- **Backend: .NET 10 y ASP.NET Core** | Por qué: es lo que piden las vacantes y ya lo conoce | Por qué no otra: Go o Node no cubren esa brecha.
- **Frontend: React, TypeScript, Vite y TanStack Query** | Por qué: en las vacantes se pide React con .NET | Por qué no Next.js: no se necesita renderizado en servidor, porque casi todo está detrás de un inicio de sesión.
- **Datos: PostgreSQL en Supabase y EF Core Code-First con Npgsql** | Por qué: plan gratuito sin tarjeta y las migraciones quedan versionadas | Por qué no MongoDB: los datos son relacionales y el dinero exige transacciones.
- **Supabase solo como base de datos** | No se usan Supabase Auth ni su API automática: la autenticación y las reglas de negocio viven en .NET.
- **Autenticación: ASP.NET Core Identity y JWT** | Access token en memoria y refresh token en cookie `httpOnly` | Por qué no un proveedor externo: en entrevista debe poder explicar el flujo.
- **Capas: Domain, Application, Infrastructure y Api** | Por qué: las reglas de estado quedan aisladas y se prueban sin base de datos | Por qué no MediatR ni CQRS: no resuelven un problema actual.
- **Sin Redis ni microservicios en este proyecto** | Por qué: ningún caso los necesita | Los microservicios se practicarán en un proyecto posterior con una razón real.

### Modelo de datos inicial

`User`, `Client`, `Project`, `ProjectVersion`, `Milestone`, `ClientAccessLink`, `Payment`, `ProjectEvent`.

- Toda entidad del freelancer tiene `OwnerId`, y cada consulta filtra por él.
- El dinero usa `numeric(18,2)` en la base y `decimal` en C#, nunca `float`.

### Estructura

```text
hito/
├── src/
│   ├── Hito.Domain/
│   ├── Hito.Application/
│   ├── Hito.Infrastructure/
│   └── Hito.Api/
├── web/
├── tests/
│   ├── Hito.UnitTests/
│   ├── Hito.IntegrationTests/
│   └── e2e/
├── k8s/
├── docs/
│   ├── decisiones/
│   └── proyecto/
├── .github/workflows/
├── compose.yaml
└── README.md
```

### Seguridad mínima

- Permisos validados en el servidor.
- Límite de intentos en inicio de sesión, enlaces públicos y creación de demos.
- Validación de entradas y URLs de evidencia solo con `http` o `https`.
- Secretos solo en GitHub Actions y en Render, nunca en el repositorio.

## Infraestructura a $0

Todo en plan gratuito y **sin registrar tarjeta**. Verifica precios y límites otra vez antes de crear recursos. Si un servicio pide tarjeta o puede generar un cobro, detente y pregunta.

- **Base de datos: Supabase Free (PostgreSQL).** Conexión por el **session pooler** (puerto 5432) con un `Maximum Pool Size` bajo en la cadena de conexión.
- **API: Render Free con el Dockerfile del repositorio.** Antes de crear el servicio, confirma que el plan gratuito acepte Dockerfile; si no, detente y propón otra opción gratuita.
- **Frontend: Vercel Hobby.** Su uso es personal y no comercial, así que sirve para este portafolio, no para venderlo como negocio.
- **Kubernetes: solo `kind`, en local y en GitHub Actions con `helm/kind-action`.**

**Arranque en frío de Render:** se apaga tras 15 minutos sin tráfico y durante el arranque puede responder HTML en vez de JSON. La interfaz llama a `/api/health/live`, muestra "Despertando la demo, puede tardar hasta un minuto" y reintenta hasta recibir la respuesta esperada. EF Core usa `EnableRetryOnFailure`.

**Pausa de Supabase:** se pausa tras una semana con poca actividad. Un flujo programado de GitHub Actions llama a `/health/ready` dos veces por semana. Como esto no garantiza evitar la pausa, el README explica cómo restaurar el proyecto y recrear los datos de la demo.

**Migraciones:** genera un bundle de EF Core y ejecútalo desde GitHub Actions contra Supabase antes de llamar al deploy hook de Render. No las ejecutes al arrancar la API.

**Límites que se declaran:** Supabase Free no incluye respaldos automáticos y Render Free no es para producción. Este despliegue es solo una demo de portafolio. Este proyecto no demuestra Azure ni AWS; no lo presentes como experiencia en esas nubes.

## Pruebas

- **Unitarias:** transiciones de estado, cálculo de totales y reglas de pago.
- **Integración:** API con PostgreSQL real mediante Testcontainers, incluido el aislamiento entre usuarios.
- **E2E con Playwright:** un solo flujo crítico: crear proyecto → aceptar → entregar → aprobar → pagar.
- **Kubernetes:** desplegar en `kind` y probar `/health/ready` y una llamada a la API.

## Etapas

Detente y espera aprobación al final de cada una.

### Etapa 0: preparación

1. Clona las referencias y resume en 10 líneas qué se reutiliza.
2. Verifica las herramientas instaladas: .NET 10, Node, Docker, Git y GitHub CLI. Dile a Max qué falta.
3. Crea el repositorio, la solución, `compose.yaml` con PostgreSQL y una API con `/health/live` y `/health/ready`.
4. Crea el primer flujo de CI: compilar y probar.

### Etapa 1: despliegue temprano

1. Guía a Max para crear las cuentas de Supabase, Render y Vercel sin tarjeta. Él debe aceptar los términos y nunca debe pegarte contraseñas.
2. Confirma que Render Free acepte el Dockerfile antes de crear el servicio.
3. Publica la API mínima en Render y una página mínima en Vercel con el rewrite de `/api/*`, conectadas a Supabase.
4. Crea el flujo de despliegue: pruebas → migraciones con bundle → deploy hook de Render. La prueba en `kind` se inserta antes de las migraciones en la Etapa 5.
5. Crea el flujo programado que llama a `/health/ready` dos veces por semana.
6. Confirma que ningún servicio tenga tarjeta registrada y que el costo sea $0.

### Etapa 2: diseño

1. Mapea los casos Debe a un recorrido de pantallas.
2. Usa `interface-design` para definir la dirección visual, si la skill está disponible. Presenta 2 o 3 opciones para que Max elija.
3. Guarda las reglas en `docs/proyecto/07-diseno.md`.
4. Diseña el panel de cobros, el detalle del proyecto y la vista del cliente, con sus estados de carga, vacío, error y éxito. Empieza por móvil en la vista del cliente.

### Etapa 3: casos 1 a 5

Construye un caso completo a la vez, de la interfaz a la base de datos.

### Etapa 4: casos 6 a 10

Termina el flujo de cobro, el historial y la demo con datos realistas.

### Etapa 5: calidad y Kubernetes

1. Completa las pruebas y agrega la prueba en `kind` al flujo de despliegue, entre las pruebas y las migraciones.
2. Revisa accesibilidad, rendimiento y seguridad. Usa `vercel-web-quality` y `code-review` si están disponibles.

### Etapa 6: presentación

1. Escribe el README: problema, demo, capturas, arquitectura, decisiones, cómo correrlo y límites conocidos.
2. Escribe 3 viñetas para el CV que digan solo lo que el repositorio demuestra. Kubernetes se describe como "probado en kind dentro de CI", no como producción. No menciones Azure ni AWS.
3. Haz con Max una entrevista simulada de 10 preguntas sobre el proyecto.

## Definición de terminado

- [ ] Los 10 casos Debe cumplen su criterio.
- [ ] La demo pública funciona desde una ventana privada, después de un arranque en frío.
- [ ] El CI pasa y el despliegue es automático.
- [ ] El costo mensual es $0 y ningún servicio tiene tarjeta registrada.
- [ ] El README permite entender el proyecto en menos de 2 minutos.
- [ ] Max puede explicar cada decisión sin ayuda.

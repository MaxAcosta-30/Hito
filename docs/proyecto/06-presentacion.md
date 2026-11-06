# Etapa 6 · Presentación

**README:** rama `etapa-6/presentacion`. Problema, demo, capturas, arquitectura (diagramas Mermaid), decisiones con enlace a cada ADR, pruebas, cómo correrlo y límites conocidos.
- Capturas en `docs/capturas/`, tomadas con Playwright sobre la build de producción en local con los datos de la demo (390 px, tema claro).
- Cifras verificadas el 2026-11-06 con `dotnet test`: 51 unitarias y 50 de integración en verde.
- Licencia MIT (decisión de Max, 2026-11-06): código abierto. ADR 0004 aceptado.

## Viñetas para el CV

Solo dicen lo que el repositorio demuestra.

1. Construí y publiqué **Hito**, una aplicación full stack con **ASP.NET Core 10, React 19 con TypeScript y PostgreSQL** para que un freelancer acuerde hitos con su cliente y controle sus cobros. Las reglas de estado viven en el dominio y responden 409 ante transiciones inválidas, y el aislamiento entre usuarios está verificado con pruebas de integración.
2. Implementé la autenticación con **ASP.NET Core Identity y JWT** (access token en memoria y refresh token rotado en cookie `httpOnly`), además de enlaces privados para clientes sin cuenta: token de 256 bits guardado solo como hash SHA-256, que expira y se puede revocar.
3. Automaticé **CI/CD con GitHub Actions**: 51 pruebas unitarias, 50 de integración con Testcontainers, un e2e con Playwright y una prueba de despliegue en **Kubernetes con kind dentro de CI**. Después se aplican las migraciones de EF Core (bundle) y se despliega en Render y Vercel a costo $0.

## Entrevista simulada

2026-11-06. Max respondió la 1 y pidió seguir con el trabajo; **las preguntas 2 a 10 siguen pendientes**.

1. *Explica Hito en 30 segundos.* Max: "Es una app para freelancers; sirve para que controlen sus pagos y su flujo de trabajo sin complicarse la vida."
   **Corrección:** va en la dirección correcta, pero es genérica y no dice qué lo hace distinto. Respuesta modelo: "Hito resuelve el '¿qué acordamos y cuánto me deben?' del freelancer. El cliente acepta el alcance desde un enlace privado, sin cuenta, y los montos quedan congelados. Cada hito se entrega con evidencia y el cliente lo aprueba o pide cambios. El panel muestra por cobrar, aprobado sin pagar y vencido. Está hecho con .NET 10, React y PostgreSQL, con CI que prueba hasta en Kubernetes."
2. ¿Por qué Domain, Application, Infrastructure y Api, y por qué no MediatR ni repositorios?
3. Recorre el flujo de autenticación: dónde vive cada token y qué pasa cuando expira el access token.
4. Alguien cambia el id en la URL para leer un proyecto ajeno: ¿qué responde la API, por qué y dónde está el filtro?
5. ¿Qué significa que el alcance "queda congelado" y cómo se modela en la base?
6. Dos pestañas registran el mismo pago a la vez: ¿qué pasa?
7. ¿Por qué `decimal` y `numeric(18,2)`? Da un ejemplo de fallo con `double`.
8. Explica el pipeline de CI/CD y por qué las migraciones no corren al arrancar la API.
9. ¿Qué demuestra la prueba en kind y qué no?
10. ¿Qué cambiarías primero con 1,000 freelancers pagando?

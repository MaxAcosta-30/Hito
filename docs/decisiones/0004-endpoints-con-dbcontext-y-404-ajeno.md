# 0004 · Endpoints con DbContext directo y 404 para datos ajenos

**Estado:** aceptada · 2026-11-06

**Decisión:**
- Los endpoints mínimos de `Hito.Api` usan `HitoDbContext` directamente. Las reglas de negocio viven en las entidades de `Hito.Domain` y se prueban sin base de datos.
- La validación de entrada (longitudes, formatos) se hace en el endpoint con `Validacion` y responde 400 con `ValidationProblem`, igual que `/auth`.
- Un recurso de otro usuario responde **404**, no 403: el filtro global por `OwnerId` hace que simplemente no exista para quien pregunta.

**Por qué no repositorios ni servicios de aplicación:** `DbContext` ya es unidad de trabajo y repositorio; otra capa encima solo reenvía llamadas. Si un caso llega a necesitarla (por ejemplo, reutilizar lógica entre endpoints), se agrega ahí.
**Por qué no FluentValidation ni la validación automática de .NET 10:** las reglas son pocas y la validación manual da los mismos nombres de campo que ya usa la web.
**Por qué 404 y no 403:** un 403 confirmaría que el id existe y pertenece a otra persona.

# 0003 · Autenticación: Identity, JWT en memoria y refresh token en cookie

**Estado:** aceptada · 2026-11-05

**Decisión:**
- ASP.NET Core Identity (`IdentityUserContext`, sin roles) para usuarios y contraseñas; endpoints propios en `/auth`.
- Access token JWT HS256 de 15 min, guardado solo en memoria del navegador. Clave en `Jwt__Clave` (variable de entorno).
- Refresh token aleatorio de 256 bits en cookie `httpOnly`, `Secure`, `SameSite=Strict`, `Path=/api/auth`. En la base solo su hash SHA-256; dura 7 días, rota en cada uso y reutilizar uno ya rotado revoca todas las sesiones del usuario.
- Contraseña mínima de 10 caracteres sin reglas de símbolos (NIST 800-63B). 5 fallos bloquean la cuenta 5 min. Registro limitado a 10 por minuto en total.
- El `OwnerId` sale del claim `sub` (`IUsuarioActual`) y `HitoDbContext` filtra por él a toda entidad `IPerteneceAUsuario`.

**Por qué no `MapIdentityApi`:** emite tokens opacos y devuelve el refresh token en el cuerpo; no es JWT ni cookie.
**Por qué no guardar el JWT en `localStorage`:** cualquier XSS lo robaría. En memoria se pierde al recargar y se recupera con la cookie.
**Por qué no un proveedor externo (Auth0, Supabase Auth):** el objetivo es poder explicar el flujo completo en una entrevista.

**Límites conocidos:** el límite de registros es global (un abuso bloquea a todos un minuto); el bloqueo por cuenta permite que alguien bloquee a propósito una cuenta ajena 5 min. Aceptable para una demo.

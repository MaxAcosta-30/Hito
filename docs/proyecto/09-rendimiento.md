# Rendimiento (2026-11-06)

Max reportó que Hito se sentía lento. Se midió antes de cambiar nada.

## Medición en producción (antes)

| Qué | Tiempo |
|---|---|
| Landing (HTML) y LCP | 0.25 s · 1.0 s |
| API en caliente vía Vercel → Render → Supabase | 0.4–1.1 s por petición (~0.3 s es solo el viaje de red) |
| Primera visita a cada pantalla | 0.8–1.2 s |
| Segunda visita a la misma pantalla | 0.06 s |
| Arranque en frío de Render (tras 15 min sin uso) | hasta ~1 min |
| JS y CSS con hash | `Cache-Control: max-age=0, must-revalidate`: se revalidaban en cada visita |

**Causa principal en la navegación:** una cascada. Primero se descargaba el código de la pantalla, **después** se pedían sus datos y, además, `React.lazy` mostraba el esqueleto al menos 300 ms, porque React lo mantiene ese mínimo para evitar parpadeos.

## Cambios

1. **Pantallas precargadas** (`web/src/paginas/pantallas.tsx`): el código de las 7 pantallas internas se descarga cuando el navegador queda libre y, una vez listo, la pantalla se dibuja directamente, sin `Suspense` ni el mínimo de 300 ms.
2. **Datos precargados al pasar el cursor** (`usePrecarga`): enlaces de la barra, filas y renglones de proyectos, "Necesita tu atención" y la pestaña Historial piden sus datos con `onMouseEnter`, `onFocus` y `onTouchStart`. Cada consulta se define una vez con `queryOptions` y la comparten el hook y la precarga. `staleTime` de 30 s para no repetir lo recién precargado; las mutaciones siguen invalidando.
3. **Caché permanente** para `/assets/*` en `vercel.json` (`max-age=31536000, immutable`): los nombres llevan hash, así que un despliegue nuevo cambia el nombre.
4. **API con ReadyToRun** (`Dockerfile`): el código va precompilado. Medido en local con los límites de Render Free (`--cpus=0.1 --memory=512m`), dos corridas cada uno:

| Paso | Antes | ReadyToRun |
|---|---|---|
| Responde `/health/live` | 3.6 s | 3.5 s |
| Lista con la base (`ready`) | 6.0–6.6 s | 3.1–3.2 s |
| Primera `POST /demo` | 6.4–6.7 s | 3.8–4.0 s |
| Primer `GET /cobros` | 2.3–2.9 s | 2.1 s |

La imagen pasa de 172 a 185 MB.

## Resultado en local

Navegación con datos ya precargados: Panel → Proyectos **34 ms**, Proyectos → detalle **63 ms** y → Clientes **65 ms**, sin esqueleto (antes ~800 ms en producción). En producción hay que sumar la red cuando el clic llega antes que la respuesta precargada.

## API despierta de día (decisión de Max, 2026-11-06)

- `.github/workflows/despertar-render.yml` llama a `/health/live` cada 10 min de 7:00 a 23:00, hora de México: unas 496 de las 750 h gratis al mes. De noche la API se duerme para dejar margen; si se agotaran las horas, Render la suspendería hasta el mes siguiente sin cobrar.
- GitHub puede retrasar o saltarse ejecuciones programadas: reduce los arranques en frío, no los elimina. La pantalla "Despertando" sigue cubriendo ese caso.

## Pendiente (decisión de Max)

- **Modelo compilado de EF Core** (`dbcontext optimize`): acortaría el arranque, pero hay que regenerarlo con cada cambio del modelo. Se dejó fuera por el costo de mantenimiento.

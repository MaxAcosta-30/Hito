import { Link } from 'react-router'
import { usePanel, type HitoQueAtender } from '../api/cobros'
import { useClientes } from '../api/clientes'
import { ErrorCarga } from '../componentes/Estados'
import { GraficaCobros } from '../componentes/GraficaCobros'
import { LibroAnimado } from '../componentes/LibroAnimado'
import { useCifraAnimada } from '../componentes/useCifraAnimada'
import { Sello } from '../componentes/Sello'
import { dinero, fechaCorta } from '../formato'

// Panel de cobros. Foco: la cifra "Te deben"; debajo, solo lo que requiere acción.
export function Cobros() {
  const panel = usePanel()

  if (panel.isPending) {
    return (
      <div aria-busy="true" aria-label="Cargando">
        <div className="hoja">
          <div className="rotulo">Te deben</div>
          <div className="esqueleto cifra-e" />
          <div className="esqueleto corto" />
        </div>
        <div className="hoja">
          <div className="esqueleto" />
          <div className="esqueleto corto" />
          <div className="esqueleto" />
        </div>
      </div>
    )
  }
  if (panel.isError) return <ErrorCarga error={panel.error} onReintentar={() => panel.refetch()} />

  const p = panel.data
  if (p.proyectosActivos === 0) return <SinCobros />

  return (
    <>
      <h1 className="solo-lectores">Cobros</h1>
      <div className="hoja">
        <div className="rotulo">Te deben</div>
        <Cifra valor={p.porCobrar} />
        <div className="meta">
          En {p.proyectosActivos} {p.proyectosActivos === 1 ? 'proyecto activo' : 'proyectos activos'}
        </div>
        <div className="desglose">
          <div>
            <span className="rotulo">Aprobado sin pagar</span>
            <strong>{dinero(p.aprobadoSinPagar)}</strong>
          </div>
          <div className={p.vencido > 0 ? 'alerta' : undefined}>
            <span className="rotulo">Vencido</span>
            <strong>{dinero(p.vencido)}</strong>
          </div>
        </div>
      </div>
      {p.historial.some((m) => m.acordado > 0 || m.cobrado > 0) && (
        <div className="hoja">
          <div className="rotulo">Acordado vs. cobrado</div>
          <p className="meta">Acumulado de los últimos 6 meses. Si la línea de cobrado se queda abajo, hay cobros atrasados.</p>
          <GraficaCobros historial={p.historial} />
        </div>
      )}
      <div className="hoja">
        <div className="rotulo">Necesita tu atención</div>
        {p.atencion.length === 0 ? (
          <p className="meta">Nada pendiente por ahora. Cuando entregues un hito o tu cliente lo apruebe, aparecerá aquí.</p>
        ) : (
          p.atencion.map((a) => <Renglon key={a.hitoId} hito={a} />)
        )}
      </div>
    </>
  )
}

const motivos: Record<HitoQueAtender['motivo'], (a: HitoQueAtender) => string> = {
  Vencido: (a) => `venció el ${fechaCorta(a.fechaEntrega)}`,
  AprobadoSinPagar: () => 'aprobado, falta registrar el pago',
  CambiosSolicitados: () => 'tu cliente pidió cambios',
  EsperaRevision: () => 'entregado, esperando a tu cliente',
}

function Renglon({ hito }: { hito: HitoQueAtender }) {
  return (
    <div className="renglon">
      <b>
        <Link to={`/proyectos/${hito.proyectoId}`}>{hito.titulo}</Link>
      </b>
      <span className="monto">{dinero(hito.pendiente)}</span>
      <small>
        {hito.cliente} · {hito.proyecto} · {motivos[hito.motivo](hito)}
      </small>
      <Sello estado={hito.estado} vencido={hito.motivo === 'Vencido'} />
      {hito.estado === 'Aprobado' && (
        <div className="acciones">
          <Link className="btn secundario" to={`/proyectos/${hito.proyectoId}`}>
            Registrar pago
          </Link>
        </div>
      )}
    </div>
  )
}

// La cifra sube al cargar y al cambiar (por ejemplo, tras registrar un pago).
// El lector de pantalla lee solo el valor final, no cada cuadro de la animación.
function Cifra({ valor }: { valor: number }) {
  const mostrado = useCifraAnimada(valor)
  return (
    <>
      <div className="cifra" aria-hidden="true">
        {dinero(Math.round(mostrado * 100) / 100)}
      </div>
      <span className="solo-lectores">{dinero(valor)}</span>
    </>
  )
}

function SinCobros() {
  const clientes = useClientes()
  const sinClientes = clientes.data?.length === 0
  return (
    <div className="hoja vacio">
      <h3>Aún no hay nada por cobrar</h3>
      <p className="meta">Crea un proyecto con sus hitos y envíaselo a tu cliente para que acepte el alcance. Aquí verás cuánto te deben.</p>
      <Link className="btn primario" to={sinClientes ? '/clientes/nuevo' : '/proyectos/nuevo'}>
        {sinClientes ? 'Primero agrega un cliente' : 'Crear proyecto'}
      </Link>
      {/* Vista previa tenue: anticipa cómo se verá el panel con datos. */}
      <div className="vista-ejemplo" aria-hidden="true">
        <span className="rotulo-ejemplo">EJEMPLO</span>
        <LibroAnimado />
      </div>
    </div>
  )
}

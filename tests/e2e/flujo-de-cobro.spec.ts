import { expect, test, type Page } from '@playwright/test'

// El flujo crítico: crear proyecto → el cliente acepta → entregar → el cliente aprueba → registrar el pago.
// Freelancer y cliente usan contextos separados: el cliente no tiene cuenta ni cookie de sesión.

const enDias = (dias: number) => new Date(Date.now() + dias * 86_400_000).toISOString().slice(0, 10)

function renglon(pagina: Page, titulo: string) {
  return pagina.locator('.renglon').filter({ has: pagina.getByText(titulo, { exact: true }) })
}

test('crear proyecto, aceptar, entregar, aprobar y pagar', async ({ page: freelancer, browser }) => {
  await test.step('el freelancer crea su cuenta', async () => {
    await freelancer.goto('/registro')
    await freelancer.getByLabel('Tu nombre').fill('Ana Freelancer')
    await freelancer.getByLabel('Correo').fill(`ana.${Date.now()}@e2e.test`)
    await freelancer.getByLabel('Contraseña').fill('una frase larga de prueba')
    await freelancer.getByRole('button', { name: 'Crear cuenta' }).click()
    await expect(freelancer).toHaveURL(/\/cobros$/)
  })

  await test.step('registra un cliente', async () => {
    await freelancer.goto('/clientes/nuevo')
    await freelancer.getByLabel('Nombre', { exact: true }).fill('Panadería Rosales')
    await freelancer.getByRole('button', { name: 'Guardar cliente' }).click()
    await expect(freelancer).toHaveURL(/\/clientes$/)
  })

  await test.step('crea un proyecto con dos hitos', async () => {
    await freelancer.goto('/proyectos/nuevo')
    await freelancer.getByLabel('Nombre del proyecto').fill('Tienda en línea')
    await freelancer.locator('#hito-0-titulo').fill('Diseño')
    await freelancer.locator('#hito-0-monto').fill('10000')
    await freelancer.locator('#hito-0-fecha').fill(enDias(30))
    await freelancer.locator('#hito-0-entregable').fill('Maquetas de la tienda')
    await freelancer.getByRole('button', { name: 'Agregar hito' }).click()
    await freelancer.locator('#hito-1-titulo').fill('Desarrollo')
    await freelancer.locator('#hito-1-monto').fill('5000')
    await freelancer.locator('#hito-1-fecha').fill(enDias(60))
    await freelancer.locator('#hito-1-entregable').fill('Tienda publicada')
    await freelancer.getByRole('button', { name: 'Crear proyecto' }).click()
    await expect(freelancer).toHaveURL(/\/proyectos\/[0-9a-f-]{36}$/)
    // El total lo calcula el servidor.
    await expect(freelancer.locator('.totales')).toContainText('$15,000.00')
  })

  const enlace = await test.step('envía el alcance y obtiene el enlace privado', async () => {
    await freelancer.getByRole('button', { name: 'Enviar versión 1 al cliente' }).click()
    const campo = freelancer.getByLabel(/Comparte este enlace/)
    await expect(campo).toHaveValue(/\/p\/.+/)
    return campo.inputValue()
  })

  const contextoCliente = await browser.newContext({ ...test.info().project.use })
  const cliente = await contextoCliente.newPage()

  await test.step('el cliente abre el enlace y acepta el alcance', async () => {
    await cliente.goto(enlace)
    await expect(cliente.getByRole('heading', { name: 'Tienda en línea' })).toBeVisible()
    await cliente.getByRole('button', { name: /Aceptar alcance · \$15,000\.00/ }).click()
    await expect(cliente.getByRole('status')).toContainText('ACEPTADO')
  })

  await test.step('el freelancer entrega el primer hito', async () => {
    await freelancer.reload()
    await renglon(freelancer, 'Diseño').getByRole('button', { name: 'Marcar como entregado' }).click()
    const dialogo = freelancer.getByRole('dialog', { name: /Entregar «Diseño»/ })
    await dialogo.getByLabel('Enlace a la entrega').fill('https://example.com/maquetas')
    await dialogo.getByRole('button', { name: 'Entregar', exact: true }).click()
    await expect(renglon(freelancer, 'Diseño')).toContainText('esperando revisión')
  })

  await test.step('el cliente aprueba la entrega', async () => {
    await cliente.reload()
    await expect(cliente.getByRole('link', { name: /Ver la entrega/ })).toHaveAttribute('href', 'https://example.com/maquetas')
    await cliente.getByRole('button', { name: 'Aprobar entrega' }).click()
    await expect(cliente.getByRole('status')).toContainText('APROBADO')
  })

  await test.step('el freelancer registra el pago completo del hito', async () => {
    await freelancer.reload()
    await renglon(freelancer, 'Diseño').getByRole('button', { name: 'Registrar pago' }).click()
    const dialogo = freelancer.getByRole('dialog', { name: 'Registrar pago' })
    await expect(dialogo.getByLabel('Monto recibido (MXN)')).toHaveValue('10000.00')
    await dialogo.getByRole('button', { name: /Registrar \$10,000\.00/ }).click()
    await expect(renglon(freelancer, 'Diseño')).toContainText('Pagado completo')
    await expect(freelancer.locator('.totales')).toContainText('Por cobrar$5,000.00')
  })

  await test.step('el panel de cobros cuadra con lo registrado', async () => {
    await freelancer.getByRole('link', { name: 'Cobros' }).click()
    await expect(freelancer.locator('.cifra')).toHaveText('$5,000.00')
    await expect(freelancer.locator('.desglose')).toContainText('Aprobado sin pagar$0.00')
    await expect(freelancer.locator('.desglose')).toContainText('Vencido$0.00')
  })

  await contextoCliente.close()
})

namespace Hito.Domain.Proyectos;

public enum EstadoProyecto { Borrador, Enviado, Aceptado, Completado, Cancelado }

// Borrador: se puede editar. Enviada: espera la respuesta del cliente. Aceptada: sus términos quedan congelados.
// Reemplazada: se envió, pero una versión posterior la sustituyó antes de que el cliente la aceptara.
public enum EstadoVersion { Borrador, Enviada, Aceptada, Reemplazada }

public enum EstadoHito { Pendiente, Entregado, CambiosSolicitados, Aprobado, Pagado }

namespace EcommerceNet.Core.Enums;

/// <summary>
/// Estados posibles de una orden de compra
/// </summary>
public enum EstadoOrden
{
    Pendiente = 0,      // recién creada
    Pagada = 1,         // pago confirmado
    EnPreparacion = 2,  // preparando envío
    Enviada = 3,        // en camino
    Entregada = 4,      // cliente recibió
    Cancelada = 5       // cancelada
}

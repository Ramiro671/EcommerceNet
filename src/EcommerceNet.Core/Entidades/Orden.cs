using EcommerceNet.Core.Enums;

namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Orden de compra — registro permanente de una compra completada.
/// </summary>
public class Orden
{
    public int Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;
    public decimal Total { get; set; }
    public string DireccionEnvio { get; set; } = string.Empty;

    // Relaciones
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public List<OrdenDetalle> Detalles { get; set; } = new();

    public void GenerarNumeroOrden()
    {
        NumeroOrden = $"ORD-{FechaCreacion:yyyyMMdd}-{Id:D4}";
    }

    public void RecalcularTotal()
    {
        Total = Detalles.Sum(d => d.Subtotal);
    }

    public bool SePuedeCancelar()
    {
        return Estado == EstadoOrden.Pendiente || Estado == EstadoOrden.Pagada;
    }

    /// <summary>Cancela la orden y devuelve stock</summary>
    public void Cancelar()
    {
        if (!SePuedeCancelar())
            throw new InvalidOperationException(
                $"No se puede cancelar una orden en estado '{Estado}'");

        Estado = EstadoOrden.Cancelada;
        foreach (var detalle in Detalles)
            detalle.Producto?.AumentarStock(detalle.Cantidad);
    }
}

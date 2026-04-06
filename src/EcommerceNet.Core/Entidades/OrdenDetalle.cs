namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Línea de una orden — foto del momento de la compra.
/// </summary>
public class OrdenDetalle
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public int OrdenId { get; set; }
    public Orden? Orden { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public void CalcularSubtotal()
    {
        Subtotal = PrecioUnitario * Cantidad;
    }
}

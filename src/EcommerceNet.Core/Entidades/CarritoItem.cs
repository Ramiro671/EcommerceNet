namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Un item dentro del carrito.
/// Guarda el precio al momento de agregarlo (puede cambiar después).
/// </summary>
public class CarritoItem
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    // Relaciones
    public int CarritoId { get; set; }
    public Carrito? Carrito { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    /// <summary>Precio × cantidad</summary>
    public decimal CalcularSubtotal() => PrecioUnitario * Cantidad;
}

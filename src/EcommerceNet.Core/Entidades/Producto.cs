namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Producto a la venta. Entidad central del sistema.
/// Contiene lógica de validación de stock.
/// </summary>
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relación con categoría
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // --- Lógica de negocio ---

    /// <summary>¿Hay stock suficiente para esta cantidad?</summary>
    public bool TieneStockSuficiente(int cantidad)
    {
        return Activo && Stock >= cantidad;
    }

    /// <summary>Reduce stock tras una compra</summary>
    public void ReducirStock(int cantidad)
    {
        // 🔴 BP-32: REDUCIR STOCK. Inspeccionar: Stock ANTES, cantidad a reducir, Stock DESPUÉS (=Stock-cantidad)
        // 🔴 BP-43: Stock insuficiente (si llega aquí). Inspeccionar: Stock actual, cantidad pedida
        if (!TieneStockSuficiente(cantidad))
            throw new InvalidOperationException(
                $"Stock insuficiente para '{Nombre}'. Disponible: {Stock}, solicitado: {cantidad}");
        Stock -= cantidad;
    }

    /// <summary>Aumenta stock (reabastecimiento o cancelación)</summary>
    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero");
        Stock += cantidad;
    }
}

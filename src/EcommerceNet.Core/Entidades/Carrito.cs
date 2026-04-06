namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Carrito de compras. Cada usuario tiene uno solo.
/// Contiene TODA la lógica de agregar, quitar y actualizar items.
/// </summary>
public class Carrito
{
    public int Id { get; set; }
    public DateTime UltimaModificacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public List<CarritoItem> Items { get; set; } = new();

    // --- Cálculos ---

    public decimal CalcularTotal() => Items.Sum(i => i.CalcularSubtotal());
    public int TotalProductos() => Items.Sum(i => i.Cantidad);
    public bool EstaVacio() => Items.Count == 0;

    // --- Operaciones ---

    /// <summary>
    /// Agrega un producto. Si ya existe, incrementa la cantidad.
    /// </summary>
    public void AgregarProducto(Producto producto, int cantidad = 1)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero");

        if (!producto.TieneStockSuficiente(cantidad))
            throw new InvalidOperationException(
                $"Stock insuficiente para '{producto.Nombre}'");

        var existente = Items.FirstOrDefault(i => i.ProductoId == producto.Id);

        if (existente != null)
        {
            existente.Cantidad += cantidad;
        }
        else
        {
            Items.Add(new CarritoItem
            {
                ProductoId = producto.Id,
                Producto = producto,
                Cantidad = cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        UltimaModificacion = DateTime.UtcNow;
    }

    /// <summary>Actualiza cantidad. Si es 0, elimina el item.</summary>
    public void ActualizarCantidad(int productoId, int nuevaCantidad)
    {
        var item = Items.FirstOrDefault(i => i.ProductoId == productoId)
            ?? throw new InvalidOperationException("Producto no encontrado en el carrito");

        if (nuevaCantidad <= 0)
            Items.Remove(item);
        else
            item.Cantidad = nuevaCantidad;

        UltimaModificacion = DateTime.UtcNow;
    }

    /// <summary>Elimina un producto del carrito</summary>
    public void EliminarProducto(int productoId)
    {
        var item = Items.FirstOrDefault(i => i.ProductoId == productoId)
            ?? throw new InvalidOperationException("Producto no encontrado en el carrito");

        Items.Remove(item);
        UltimaModificacion = DateTime.UtcNow;
    }

    /// <summary>Vacía el carrito completamente</summary>
    public void Vaciar()
    {
        Items.Clear();
        UltimaModificacion = DateTime.UtcNow;
    }
}

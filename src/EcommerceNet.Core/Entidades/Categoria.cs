namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Categoría de productos (ej: Electrónica, Ropa, Hogar)
/// </summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;

    // Relación: una categoría tiene muchos productos
    public List<Producto> Productos { get; set; } = new();

    /// <summary>Cuenta productos activos en esta categoría</summary>
    public int TotalProductosActivos()
    {
        return Productos.Count(p => p.Activo);
    }
}

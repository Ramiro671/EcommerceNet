namespace EcommerceNet.Core.DTOs;

public class OrdenDto
{
    public int Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrdenDetalleDto> Detalles { get; set; } = new();
}

public class OrdenDetalleDto
{
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CrearOrdenDto
{
    public string DireccionEnvio { get; set; } = string.Empty;
}

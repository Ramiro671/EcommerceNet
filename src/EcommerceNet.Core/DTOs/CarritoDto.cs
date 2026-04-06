namespace EcommerceNet.Core.DTOs;

public class CarritoDto
{
    public int Id { get; set; }
    public List<CarritoItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public int TotalProductos { get; set; }
}

public class CarritoItemDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

public class AgregarAlCarritoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; } = 1;
}

using EcommerceNet.Core.DTOs;

namespace EcommerceNet.Core.Servicios;

public interface ICarritoServicio
{
    Task<Resultado<CarritoDto>> ObtenerCarritoAsync(int usuarioId);
    Task<Resultado<CarritoDto>> AgregarProductoAsync(int usuarioId, AgregarAlCarritoDto dto);
    Task<Resultado<CarritoDto>> ActualizarCantidadAsync(int usuarioId, int productoId, int cantidad);
    Task<Resultado<CarritoDto>> EliminarProductoAsync(int usuarioId, int productoId);
    Task<Resultado<CarritoDto>> VaciarCarritoAsync(int usuarioId);
    Task<Resultado<OrdenDto>> CheckoutAsync(int usuarioId, CrearOrdenDto dto);
}

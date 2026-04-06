using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Core.Interfaces;

public interface ICarritoRepositorio
{
    Task<Carrito?> ObtenerPorUsuarioAsync(int usuarioId);
    Task AgregarAsync(Carrito carrito);
    void Actualizar(Carrito carrito);
}

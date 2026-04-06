using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Core.Interfaces;

public interface IOrdenRepositorio : IRepositorio<Orden>
{
    Task<IEnumerable<Orden>> ObtenerPorUsuarioAsync(int usuarioId);
    Task<Orden?> ObtenerConDetallesAsync(int ordenId);
}

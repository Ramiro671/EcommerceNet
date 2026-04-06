using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Core.Interfaces;

public interface IProductoRepositorio : IRepositorio<Producto>
{
    Task<IEnumerable<Producto>> BuscarPorNombreAsync(string termino);
    Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(int categoriaId);
    Task<IEnumerable<Producto>> ObtenerConStockBajoAsync(int minimo = 5);
    Task<IEnumerable<Producto>> ObtenerActivosAsync();
}

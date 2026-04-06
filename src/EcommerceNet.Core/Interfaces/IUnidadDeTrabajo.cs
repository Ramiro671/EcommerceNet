namespace EcommerceNet.Core.Interfaces;

/// <summary>Agrupa repositorios en una sola transacción</summary>
public interface IUnidadDeTrabajo : IDisposable
{
    IProductoRepositorio Productos { get; }
    ICarritoRepositorio Carritos { get; }
    IOrdenRepositorio Ordenes { get; }
    IUsuarioRepositorio Usuarios { get; }
    Task<int> GuardarCambiosAsync();
}

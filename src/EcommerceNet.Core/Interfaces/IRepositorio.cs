namespace EcommerceNet.Core.Interfaces;

/// <summary>Contrato genérico CRUD para cualquier entidad</summary>
public interface IRepositorio<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}
